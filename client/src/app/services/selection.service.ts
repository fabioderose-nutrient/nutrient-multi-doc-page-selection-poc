import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { PageSelection } from '../models/page-selection';

@Injectable({ providedIn: 'root' })
export class SelectionService {
  private selectionMap = new Map<string, Set<number>>();
  private selectionSubject = new BehaviorSubject<PageSelection[]>([]);

  selection$ = this.selectionSubject.asObservable();

  get totalSelected(): number {
    let total = 0;
    for (const pages of this.selectionMap.values()) {
      total += pages.size;
    }
    return total;
  }

  isSelected(documentId: string, pageNumber: number): boolean {
    return this.selectionMap.get(documentId)?.has(pageNumber) ?? false;
  }

  toggle(documentId: string, pageNumber: number): void {
    if (!this.selectionMap.has(documentId)) {
      this.selectionMap.set(documentId, new Set());
    }
    const docSet = this.selectionMap.get(documentId)!;
    if (docSet.has(pageNumber)) {
      docSet.delete(pageNumber);
      if (docSet.size === 0) this.selectionMap.delete(documentId);
    } else {
      docSet.add(pageNumber);
    }
    this.emit();
  }

  selectAll(documentId: string, pageCount: number): void {
    const newSet = new Set<number>();
    for (let i = 1; i <= pageCount; i++) newSet.add(i);
    this.selectionMap.set(documentId, newSet);
    this.emit();
  }

  deselectAll(documentId: string): void {
    this.selectionMap.delete(documentId);
    this.emit();
  }

  isAllSelected(documentId: string, pageCount: number): boolean {
    const docSet = this.selectionMap.get(documentId);
    return docSet?.size === pageCount;
  }

  getSelectedPages(): PageSelection[] {
    const pages: PageSelection[] = [];
    for (const [documentId, pageNumbers] of this.selectionMap) {
      for (const pageNumber of [...pageNumbers].sort((a, b) => a - b)) {
        pages.push({ documentId, pageNumber });
      }
    }
    return pages;
  }

  private emit(): void {
    this.selectionSubject.next(this.getSelectedPages());
  }
}
