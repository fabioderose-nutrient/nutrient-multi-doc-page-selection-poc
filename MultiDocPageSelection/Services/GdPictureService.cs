using GdPicture14;
using GdPicture14.Imaging;
using MultiDocPageSelection.Models;

namespace MultiDocPageSelection.Services;

public class GdPictureService
{
    private readonly string _documentsPath;

    public GdPictureService(string documentsPath)
    {
        _documentsPath = documentsPath;
    }

    // Document IDs are file names: strip any folder part so a request cannot leave the Documents folder.
    private string ResolvePath(string documentId) =>
        Path.Combine(_documentsPath, Path.GetFileName(documentId));

    public List<DocumentInfo> GetDocuments()
    {
        var documents = new List<DocumentInfo>();
        var supportedExtensions = new[] { ".pdf", ".tif", ".tiff" };

        foreach (var file in Directory.GetFiles(_documentsPath))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!supportedExtensions.Contains(ext))
                continue;

            var fileName = Path.GetFileName(file);
            int pageCount = 0;

            if (ext == ".pdf")
            {
                using var pdf = new GdPicturePDF();
                if (pdf.LoadFromFile(file, false) == GdPictureStatus.OK)
                {
                    pageCount = pdf.GetPageCount();
                    pdf.CloseDocument();
                }
            }
            else
            {
                using var imaging = new GdPictureImaging();
                int imageId = imaging.CreateGdPictureImageFromFile(file);
                if (imageId != 0)
                {
                    pageCount = imaging.GetPageCount(imageId);
                    if (pageCount <= 0) pageCount = 1;
                    imaging.ReleaseGdPictureImage(imageId);
                }
            }

            documents.Add(new DocumentInfo
            {
                Id = fileName,
                FileName = fileName,
                PageCount = pageCount
            });
        }

        return documents.OrderBy(d => d.FileName).ToList();
    }

    public byte[]? GetThumbnail(string documentId, int pageNumber, int width = 150)
    {
        var filePath = ResolvePath(documentId);
        if (!File.Exists(filePath))
            return null;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        using var imaging = new GdPictureImaging();
        int imageId = 0;

        if (ext == ".pdf")
        {
            using var pdf = new GdPicturePDF();
            if (pdf.LoadFromFile(filePath, false) != GdPictureStatus.OK)
                return null;

            pdf.SelectPage(pageNumber);

            // Calculate DPI to achieve desired thumbnail width
            float pageWidth = pdf.GetPageWidth(); // in points (1/72 inch)
            float dpi = width * 72f / pageWidth;
            dpi = Math.Max(dpi, 20f); // minimum DPI

            imageId = pdf.RenderPageToGdPictureImage(dpi, false, 10000);
            pdf.CloseDocument();
        }
        else
        {
            imageId = imaging.CreateGdPictureImageFromFile(filePath);
            if (imageId != 0 && pageNumber > 1)
            {
                imaging.TiffSelectPage(imageId, pageNumber);
            }
        }

        if (imageId == 0)
            return null;

        // Resize if needed
        int currentWidth = imaging.GetWidth(imageId);
        if (currentWidth > width)
        {
            int currentHeight = imaging.GetHeight(imageId);
            int targetHeight = (int)((float)currentHeight / currentWidth * width);
            imaging.Resize(imageId, width, targetHeight, GdPictureInterpolationMode.HighQualityBicubic);
        }

        // SaveAsPNG has no Stream overload, so use a temp file
        var tempFile = Path.GetTempFileName() + ".png";
        try
        {
            imaging.SaveAsPNG(imageId, tempFile);
            imaging.ReleaseGdPictureImage(imageId);
            return File.ReadAllBytes(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    public byte[]? ExportAsPdf(List<PageSelection> pages)
    {
        if (pages.Count == 0)
            return null;

        using var destPdf = new GdPicturePDF();
        destPdf.NewPDF();

        // Group pages by document to minimize file opens
        var grouped = pages.GroupBy(p => p.DocumentId);

        foreach (var group in grouped)
        {
            var filePath = ResolvePath(group.Key);
            if (!File.Exists(filePath))
                continue;

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".pdf")
            {
                using var srcPdf = new GdPicturePDF();
                if (srcPdf.LoadFromFile(filePath, false) != GdPictureStatus.OK)
                    continue;

                foreach (var page in group.OrderBy(p => p.PageNumber))
                {
                    destPdf.ClonePage(srcPdf, page.PageNumber);
                }
                srcPdf.CloseDocument();
            }
            else
            {
                // For TIFF: render each page and add as PDF page
                using var imaging = new GdPictureImaging();
                int imageId = imaging.CreateGdPictureImageFromFile(filePath);
                if (imageId == 0) continue;

                foreach (var page in group.OrderBy(p => p.PageNumber))
                {
                    imaging.TiffSelectPage(imageId, page.PageNumber);
                    float widthInches = imaging.GetWidth(imageId) / Math.Max(imaging.GetHorizontalResolution(imageId), 72f);
                    float heightInches = imaging.GetHeight(imageId) / Math.Max(imaging.GetVerticalResolution(imageId), 72f);

                    destPdf.NewPage(widthInches * 72, heightInches * 72);
                    string imageResName = destPdf.AddImageFromGdPictureImage(imageId, false, false);
                    destPdf.DrawImage(imageResName, 0, 0, widthInches * 72, heightInches * 72);
                }
                imaging.ReleaseGdPictureImage(imageId);
            }
        }

        if (destPdf.GetPageCount() == 0)
            return null;

        using var ms = new MemoryStream();
        destPdf.SaveToStream(ms);
        return ms.ToArray();
    }

    public byte[]? ExportAsTiff(List<PageSelection> pages)
    {
        if (pages.Count == 0)
            return null;

        // Strategy: create a merged PDF first, then convert to TIFF
        var pdfBytes = ExportAsPdf(pages);
        if (pdfBytes == null)
            return null;

        // Use GdPictureDocumentConverter to convert PDF to TIFF
        using var converter = new GdPictureDocumentConverter();
        using var inputStream = new MemoryStream(pdfBytes);
        if (converter.LoadFromStream(inputStream, GdPicture14.DocumentFormat.DocumentFormatPDF) != GdPictureStatus.OK)
            return null;

        using var outputStream = new MemoryStream();
        if (converter.SaveAsTIFF(outputStream, TiffCompression.TiffCompressionLZW) != GdPictureStatus.OK)
            return null;

        return outputStream.ToArray();
    }
}
