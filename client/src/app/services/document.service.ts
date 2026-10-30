import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DocumentInfo } from '../models/document-info';
import { ExportRequest } from '../models/export-request';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  constructor(private http: HttpClient) {}

  getDocuments(): Observable<DocumentInfo[]> {
    return this.http.get<DocumentInfo[]>('/api/documents');
  }

  getThumbnailUrl(documentId: string, pageNumber: number, width = 150): string {
    return `/api/documents/${encodeURIComponent(documentId)}/pages/${pageNumber}/thumbnail?width=${width}`;
  }

  // Web SDK license key served by the backend. Empty = evaluation mode.
  getConfig(): Observable<{ licenseKey: string }> {
    return this.http.get<{ licenseKey: string }>('/api/config');
  }

  export(request: ExportRequest): Observable<Blob> {
    return this.http.post('/api/export', request, { responseType: 'blob' });
  }
}
