import { PageSelection } from './page-selection';

export interface ExportRequest {
  pages: PageSelection[];
  format: 'pdf' | 'tiff';
}
