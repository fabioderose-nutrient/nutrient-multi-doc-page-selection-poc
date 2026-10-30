namespace MultiDocPageSelection.Models;

public class ExportRequest
{
    public List<PageSelection> Pages { get; set; } = new();
    public string Format { get; set; } = "pdf"; // "pdf" or "tiff"
}
