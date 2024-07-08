import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LandingPageComponent } from './components/contents/landing-page/landing-page.component';
import { ProvaComponent } from './components/contents/prova/prova.component';
import { DataGridComponent } from './components/contents/data-grid/data-grid.component';
import { FormComponent } from './components/contents/form/form.component';

const routes: Routes = [
  { path: 'landing-page', component: LandingPageComponent },
  { path: 'prova', component: ProvaComponent },
  { path: 'products', component: DataGridComponent },
  { path: 'form', component: FormComponent }
];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

export interface Content {
  route: string;
  icon: string;
  label: string;
}

export const sideBarData: Content[] = [
  { route: "landing-page", icon: "fa fa-home", label: "Landing-page" },
  { route: "prova", icon: "fa fa-poo", label: "Prova" },
  { route: "products", icon: "fa fa-table", label: "Products" },
  { route: "form", icon: "fa fa-address-card", label: "Form" },
];