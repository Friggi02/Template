import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, of, startWith } from 'rxjs';
import { Product } from 'src/app/models/product.model';
import { CategoryService } from 'src/app/services/category.service';
import { NotificationService, NotificationType } from 'src/app/services/notification.service';
import { ProductService } from 'src/app/services/product.service';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss']
})
export class FormComponent implements OnInit{

  newProduct: Product;
  myForm: FormGroup;
  categories: any[] = [];
  filteredOptions: Observable<any[]> = of([]);

  constructor(
    fb: FormBuilder,
    private productService: ProductService,
    private notification: NotificationService,
    private categoryService: CategoryService
  ) {
    this.myForm = fb.group({
      Barcode: ["1234567890000", [Validators.required, Validators.pattern(`^[0-9]{13}$`)]],
      Name: ["prova", Validators.required],
      Category: ["", Validators.required],
      Producer: ["adsfadsa", Validators.required],
      NustriscoreGrade: ["d", [Validators.required, Validators.pattern(`^[a-e]{1}$`)]],
      NustriscoreValue: ["6", Validators.required],
      Ingredients: [""],
    });
    this.newProduct = this.myForm.value;
  }

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe({
      next: (data: any) => {
        this.categories = data.value;
        console.log(this.categories)
      },
      error: () => {
        this.notification.print("Impossibile caricare le categorie", NotificationType.Error);
      }
    });
    this.filteredOptions = this.myForm.controls['Category'].valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value))
    );
  }

  private _filter(value: string): any[] {
    const filterValue = value.toLowerCase();
    return this.categories.filter((option: any) => option.name.toLowerCase().includes(filterValue));
  }

  errorMessage(): string {
    return "This value is not valid";
  }

  getCategoryId(name: string): string {
    let category = this.categories.find(category => category.name == name);
    return category ? category.id : '';
  }

  onSubmit() {
    if (this.myForm.valid) {

      this.newProduct = {
        BarCode: this.myForm.controls["Barcode"].value,
        Name: this.myForm.controls["Name"].value,
        CategoryId: this.getCategoryId(this.myForm.controls["Category"].value),
        Producer: this.myForm.controls["Producer"].value,
        Nutriscore_Grade: this.myForm.controls["NustriscoreGrade"].value,
        Nutriscore_Value: parseInt(this.myForm.controls["NustriscoreValue"].value),
        Ingredients: this.myForm.controls["Ingredients"].value
      };
      
      this.productService.postProducts(this.newProduct).subscribe({
        next: () => {
          this.notification.print("Prodotto creato correttamente", NotificationType.Success);
        },
        error: () => {
          this.notification.print("Impossibile creare prodotto", NotificationType.Error);
        }
      });
    
    } else {
      console.log('Form is not valid');
    }
  }
}
