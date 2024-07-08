import { Component, OnInit } from '@angular/core';
import { Product } from 'src/app/models/product.model';
import {
  NotificationService,
  NotificationType,
} from 'src/app/services/notification.service';
import { ProductService } from 'src/app/services/product.service';

@Component({
  selector: 'app-data-grid',
  templateUrl: './data-grid.component.html',
  styleUrls: ['./data-grid.component.scss'],
})
export class DataGridComponent implements OnInit {
  products: Product[] = [];

  constructor(
    private productService: ProductService,
    private notification: NotificationService
  ) { }

  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: (data: any) => {
        this.products = data.value;
      },
      error: () => {
        this.notification.print(
          'Impossibile caricare i prodotti',
          NotificationType.Error
        );
      },
    });
  }
}
