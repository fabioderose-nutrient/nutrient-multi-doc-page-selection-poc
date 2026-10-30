import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  activeDocumentId: string | null = null;

  onDocumentSelected(documentId: string): void {
    this.activeDocumentId = documentId;
  }
}
