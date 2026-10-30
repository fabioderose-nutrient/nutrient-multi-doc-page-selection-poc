import { Component } from '@angular/core';
import { SelectionService } from '../../services/selection.service';
import { DocumentService } from '../../services/document.service';

@Component({
  selector: 'app-export-bar',
  templateUrl: './export-bar.component.html',
  styleUrls: ['./export-bar.component.scss']
})
export class ExportBarComponent {
  exporting = false;

  constructor(
    public selectionService: SelectionService,
    private documentService: DocumentService
  ) {}

  export(format: 'pdf' | 'tiff'): void {
    const pages = this.selectionService.getSelectedPages();
    if (pages.length === 0) return;

    this.exporting = true;

    this.documentService.export({ pages, format }).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `export.${format === 'tiff' ? 'tiff' : 'pdf'}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Export failed:', err);
        alert('Export failed. Check console for details.');
        this.exporting = false;
      }
    });
  }
}
