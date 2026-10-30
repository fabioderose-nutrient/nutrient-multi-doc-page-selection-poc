"""Generate the fictitious sample PDFs used by the demo.

Usage (from the repository root):

    python3 -m venv .venv-samples
    .venv-samples/bin/pip install reportlab
    .venv-samples/bin/python tools/generate_samples.py

The files are written to MultiDocPageSelection/Documents. All names,
companies, amounts and addresses are invented.
"""

from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.pdfgen import canvas

OUT_DIR = Path(__file__).resolve().parent.parent / "MultiDocPageSelection" / "Documents"
WIDTH, HEIGHT = letter
MARGIN = 54
FOOTER = "Fictitious sample document for a Nutrient demo. All names and figures are invented."


def header(c, title, subtitle, page, pages, color):
    c.setFillColor(color)
    c.rect(0, HEIGHT - 110, WIDTH, 110, stroke=0, fill=1)
    c.setFillColor(colors.white)
    c.setFont("Helvetica-Bold", 26)
    c.drawString(MARGIN, HEIGHT - 60, title)
    c.setFont("Helvetica", 13)
    c.drawString(MARGIN, HEIGHT - 85, subtitle)
    c.setFont("Helvetica-Bold", 13)
    c.drawRightString(WIDTH - MARGIN, HEIGHT - 60, f"Page {page} of {pages}")


def footer(c):
    c.setFillColor(colors.grey)
    c.setFont("Helvetica-Oblique", 8)
    c.drawCentredString(WIDTH / 2, 30, FOOTER)


def big_page_number(c, page, color):
    # Large watermark-style number so pages are easy to tell apart in thumbnails.
    c.saveState()
    c.setFillColor(color)
    c.setFillAlpha(0.12)
    c.setFont("Helvetica-Bold", 260)
    c.drawCentredString(WIDTH / 2, 170, str(page))
    c.restoreState()


def paragraph(c, x, y, text, size=11, leading=15, width=WIDTH - 2 * MARGIN):
    c.setFillColor(colors.black)
    c.setFont("Helvetica", size)
    words = text.split()
    line = ""
    for word in words:
        candidate = f"{line} {word}".strip()
        if c.stringWidth(candidate, "Helvetica", size) > width:
            c.drawString(x, y, line)
            y -= leading
            line = word
        else:
            line = candidate
    if line:
        c.drawString(x, y, line)
        y -= leading
    return y - 6


def heading(c, x, y, text, size=15):
    c.setFillColor(colors.black)
    c.setFont("Helvetica-Bold", size)
    c.drawString(x, y, text)
    return y - size - 8


def table(c, x, y, rows, col_widths, row_height=22):
    for r, row in enumerate(rows):
        cx = x
        if r == 0:
            c.setFillColor(colors.HexColor("#E8EAF0"))
            c.rect(x, y - row_height + 6, sum(col_widths), row_height, stroke=0, fill=1)
        c.setFillColor(colors.black)
        c.setFont("Helvetica-Bold" if r == 0 else "Helvetica", 10)
        for value, w in zip(row, col_widths):
            c.drawString(cx + 6, y - 9, str(value))
            cx += w
        c.setStrokeColor(colors.HexColor("#C5C9D6"))
        c.line(x, y - row_height + 6, x + sum(col_widths), y - row_height + 6)
        y -= row_height
    return y - 10


def new_canvas(name, title):
    c = canvas.Canvas(str(OUT_DIR / name), pagesize=letter, invariant=1)
    c.setTitle(title)
    c.setAuthor("Nutrient demo")
    c.setSubject("Fictitious sample document")
    return c


def contract():
    color = colors.HexColor("#1F4E79")
    title = "Service Agreement"
    pages = 4
    c = new_canvas("sample-contract.pdf", "Sample service agreement")
    content = [
        ("1. Parties", [
            "This Service Agreement is made between Example Holdings Ltd., 100 Sample Street, "
            "Springfield (the Client), and Placeholder Services Inc., 200 Demo Avenue, "
            "Riverside (the Provider).",
            "The Client and the Provider agree to the terms below. This agreement starts on "
            "January 1, 2026 and runs for twelve months. Either party can end it with thirty days of written notice.",
        ]),
        ("2. Scope of services", [
            "The Provider will scan, index and archive the paper records the Client sends each "
            "month. Each batch is returned as searchable PDF files within five business days.",
            "The Provider will keep a log of every batch, with the number of pages received, "
            "the number of pages delivered and any page that could not be read.",
        ]),
        ("3. Fees and payment", [
            "The Client pays a monthly fee of 1,200.00 for up to 20,000 pages. Extra pages are "
            "billed at 0.05 each. Invoices are due thirty days after the invoice date.",
            "Late payments carry a fee of 1.5 percent per month on the unpaid amount.",
        ]),
        ("4. Signatures", [
            "Signed for the Client: Alex Sample, Operations Manager. Date: ____________",
            "Signed for the Provider: Jordan Example, Account Director. Date: ____________",
        ]),
    ]
    for i, (section, paras) in enumerate(content, start=1):
        big_page_number(c, i, color)
        header(c, title, "Agreement no. SA-2026-001 (sample)", i, pages, color)
        y = HEIGHT - 150
        y = heading(c, MARGIN, y, section)
        for p in paras:
            y = paragraph(c, MARGIN, y, p)
        if i == 4:
            for label in ("Client signature", "Provider signature"):
                y -= 30
                c.setStrokeColor(colors.black)
                c.line(MARGIN, y, MARGIN + 220, y)
                c.setFont("Helvetica", 9)
                c.drawString(MARGIN, y - 12, label)
        footer(c)
        c.showPage()
    c.save()


def invoices():
    color = colors.HexColor("#2E7D32")
    pages = 3
    c = new_canvas("sample-invoices.pdf", "Sample invoices")
    monthly = ("Monthly fee (up to 20,000 pages)", "1", "1,200.00", "1,200.00")
    items = [
        [monthly, ("Indexing setup", "1", "150.00", "150.00")],
        [monthly, ("Extra pages", "1,000", "0.05", "50.00"), ("Archive storage (GB)", "40", "2.00", "80.00")],
        [monthly, ("Rush delivery", "2", "75.00", "150.00")],
    ]
    for i, rows in enumerate(items, start=1):
        big_page_number(c, i, color)
        header(c, f"Invoice INV-100{i}", "Placeholder Services Inc. (sample)", i, pages, color)
        y = HEIGHT - 150
        c.setFont("Helvetica", 11)
        c.setFillColor(colors.black)
        for line in (
            "Bill to: Example Holdings Ltd., 100 Sample Street, Springfield",
            f"Invoice date: 2026-0{i}-28    Due date: 2026-0{i + 1}-27",
            "Reference: SA-2026-001",
        ):
            c.drawString(MARGIN, y, line)
            y -= 16
        y -= 14
        total = sum(float(r[3].replace(",", "")) for r in rows)
        data = [("Description", "Quantity", "Unit price", "Amount")] + rows
        y = table(c, MARGIN, y, data, [230, 80, 90, 104])
        c.setFont("Helvetica-Bold", 12)
        c.drawRightString(WIDTH - MARGIN, y - 10, f"Total due: {total:,.2f}")
        footer(c)
        c.showPage()
    c.save()


def inspection_report():
    color = colors.HexColor("#8E3B46")
    title = "Site Inspection Report"
    pages = 3
    c = new_canvas("sample-inspection-report.pdf", "Sample site inspection report")

    # Page 1: summary
    big_page_number(c, 1, color)
    header(c, title, "Warehouse B, 300 Example Road (sample)", 1, pages, color)
    y = HEIGHT - 150
    y = heading(c, MARGIN, y, "Summary")
    y = paragraph(c, MARGIN, y,
                  "The inspection took place on March 12, 2026. The storage area, loading dock and "
                  "fire exits were checked. Two minor issues were found and one item needs action "
                  "within thirty days.")
    y = table(c, MARGIN, y - 6, [("Area", "Result"), ("Storage area", "Pass"),
                                 ("Loading dock", "Pass with notes"), ("Fire exits", "Action needed")],
              [250, 254])
    footer(c)
    c.showPage()

    # Page 2: findings
    big_page_number(c, 2, color)
    header(c, title, "Findings", 2, pages, color)
    y = HEIGHT - 150
    y = heading(c, MARGIN, y, "Findings")
    y = table(c, MARGIN, y, [
        ("No.", "Finding", "Priority", "Due"),
        ("F-01", "Exit sign above door 3 is not lit", "High", "2026-04-11"),
        ("F-02", "Pallets stored close to the sprinkler line", "Medium", "2026-04-30"),
        ("F-03", "Dock bumper on bay 2 is worn", "Low", "2026-06-30"),
    ], [50, 270, 80, 104])
    footer(c)
    c.showPage()

    # Page 3: photos and sign-off
    big_page_number(c, 3, color)
    header(c, title, "Photos and sign-off", 3, pages, color)
    y = HEIGHT - 150
    y = heading(c, MARGIN, y, "Photos")
    c.setStrokeColor(colors.grey)
    for k, label in enumerate(("Photo 1: exit sign, door 3", "Photo 2: bay 2 bumper")):
        x = MARGIN + k * 260
        c.rect(x, y - 160, 240, 160, stroke=1, fill=0)
        c.line(x, y - 160, x + 240, y)
        c.line(x, y, x + 240, y - 160)
        c.setFont("Helvetica", 9)
        c.drawString(x, y - 174, label)
    y -= 220
    y = heading(c, MARGIN, y, "Sign-off")
    paragraph(c, MARGIN, y, "Inspector: Sam Placeholder. Site contact: Casey Demo. Next inspection: September 2026.")
    footer(c)
    c.showPage()
    c.save()


if __name__ == "__main__":
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    contract()
    invoices()
    inspection_report()
    for f in sorted(OUT_DIR.glob("*.pdf")):
        print(f"{f.name}: {f.stat().st_size} bytes")
