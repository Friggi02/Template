import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './components/app/app.component';
import { AppShellComponent } from './components/app-shell/app-shell.component';
import { ProvaComponent } from './components/contents/prova/prova.component';
import { LandingPageComponent } from './components/contents/landing-page/landing-page.component';
import { DataGridComponent } from './components/contents/data-grid/data-grid.component';
import { DxDataGridModule, DxPopoverModule, DxPopupModule } from 'devextreme-angular';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { HttpClientModule } from '@angular/common/http';
import { FormComponent } from './components/contents/form/form.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { NotificationService } from './services/notification.service';
import { ProductService } from './services/product.service';
import { CategoryService } from './services/category.service';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatOptionModule } from '@angular/material/core';
 
@NgModule({
  declarations: [
    AppComponent,
    AppShellComponent,
    ProvaComponent,
    LandingPageComponent,
    DataGridComponent,
    FormComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    DxDataGridModule,
    MatButtonModule,
    MatToolbarModule,
    MatIconModule,
    MatSidenavModule,
    HttpClientModule,
    DxPopupModule,
    DxPopoverModule,
    ReactiveFormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatAutocompleteModule,
    MatOptionModule
  ],
  providers: [NotificationService, ProductService, CategoryService],
  bootstrap: [AppComponent]
})
export class AppModule { }
