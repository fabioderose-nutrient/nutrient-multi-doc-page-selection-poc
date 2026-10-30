import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { DocumentPanelComponent } from './components/document-panel/document-panel.component';
import { ViewerComponent } from './components/viewer/viewer.component';
import { ExportBarComponent } from './components/export-bar/export-bar.component';

@NgModule({
  declarations: [
    AppComponent,
    DocumentPanelComponent,
    ViewerComponent,
    ExportBarComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
