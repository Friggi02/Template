import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Config } from '../Config';
import { Product } from 'src/app/models/product.model';

@Injectable()
export class ProductService {

  private CONNECTION_PREFIX = Config.CONNECTION_PREFIX;
  
  constructor(private http: HttpClient) {}

  getProducts() {
    return this.http.get<any | null>(
      `${this.CONNECTION_PREFIX}/odata/Product?$count=true&$expand=category`
    );
  }

  postProducts(product: Product) {
    return this.http.post<any | null>(
      `${this.CONNECTION_PREFIX}/api/Product/Create`,
      product
    );
  }
}