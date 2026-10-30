import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { DocumentInfo } from '../../models/document-info';
import { DocumentService } from '../../services/document.service';
import { SelectionService } from '../../services/selection.service';

@Component({
  selector: 'app-document-panel',
  templateUrl: './document-panel.component.html',
  styleUrls: ['./document-panel.component.scss']
})
export class DocumentPanelComponent implements OnInit {
  @Output() documentSelected = new EventEmitter<string>();

  documents: DocumentInfo[] = [];
  expandedDocs = new Set<string>();
  loading = true;

  constructor(
    public documentService: DocumentService,
    public selectionService: SelectionService
  ) {}

  ngOnInit(): void {
    this.documentService.getDocuments().subscribe({
      next: (docs) => {
        this.documents = docs;
        this.loading = false;
        // Expand all by default
        docs.forEach(d => this.expandedDocs.add(d.id));
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  toggleExpand(docId: string): void {
    if (this.expandedDocs.has(docId)) {
      this.expandedDocs.delete(docId);
    } else {
      this.expandedDocs.add(docId);
    }
  }

  isExpanded(docId: string): boolean {
    return this.expandedDocs.has(docId);
  }

  onDocNameClick(doc: DocumentInfo, event: Event): void {
    event.stopPropagation();
    if (doc.fileName.toLowerCase().endsWith('.pdf')) {
      this.documentSelected.emit(doc.id);
    }
  }

  onPageClick(documentId: string, pageNumber: number): void {
    this.selectionService.toggle(documentId, pageNumber);
  }

  onSelectAllClick(doc: DocumentInfo, event: Event): void {
    event.stopPropagation();
    if (this.selectionService.isAllSelected(doc.id, doc.pageCount)) {
      this.selectionService.deselectAll(doc.id);
    } else {
      this.selectionService.selectAll(doc.id, doc.pageCount);
    }
  }

  getPages(pageCount: number): number[] {
    return Array.from({ length: pageCount }, (_, i) => i + 1);
  }

  isPdf(fileName: string): boolean {
    return fileName.toLowerCase().endsWith('.pdf');
  }
}
