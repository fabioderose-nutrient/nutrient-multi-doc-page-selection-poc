import { Component, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges, ViewChild } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DocumentService } from '../../services/document.service';

// Declare the global NutrientViewer loaded via script tag
declare const NutrientViewer: any;

@Component({
  selector: 'app-viewer',
  templateUrl: './viewer.component.html',
  styleUrls: ['./viewer.component.scss']
})
export class ViewerComponent implements OnChanges, OnDestroy {
  @Input() documentId: string | null = null;
  @ViewChild('viewerContainer', { static: true }) viewerContainer!: ElementRef<HTMLDivElement>;

  private instance: any = null;
  private currentDocId: string | null = null;
  private licenseKey: string | null = null;

  constructor(private documentService: DocumentService) {}

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    if (changes['documentId'] && this.documentId) {
      await this.loadDocument(this.documentId);
    }
  }

  ngOnDestroy(): void {
    this.unloadViewer();
  }

  private async loadDocument(documentId: string): Promise<void> {
    if (this.currentDocId === documentId && this.instance) return;
    this.currentDocId = documentId;

    this.unloadViewer();

    try {
      if (this.licenseKey === null) {
        const config = await firstValueFrom(this.documentService.getConfig()).catch(() => ({ licenseKey: '' }));
        this.licenseKey = config.licenseKey ?? '';
      }

      const configuration: Record<string, unknown> = {
        container: this.viewerContainer.nativeElement,
        document: `/documents/${encodeURIComponent(documentId)}`,
        baseUrl: `${window.location.protocol}//${window.location.host}/assets/`,
        theme: NutrientViewer.Theme.DARK
      };
      // No license key = evaluation mode (watermark).
      if (this.licenseKey) {
        configuration['licenseKey'] = this.licenseKey;
      }

      this.instance = await NutrientViewer.load(configuration);
    } catch (err) {
      console.error('Failed to load Nutrient viewer:', err);
    }
  }

  private unloadViewer(): void {
    if (this.instance) {
      NutrientViewer.unload(this.viewerContainer.nativeElement);
      this.instance = null;
    }
  }
}
