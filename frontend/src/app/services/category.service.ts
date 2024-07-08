import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Config } from '../Config';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private CONNECTION_PREFIX = Config.CONNECTION_PREFIX;
  
  constructor(private http: HttpClient) {}

  getCategories() {
    return this.http.get<any | null>(
      `${this.CONNECTION_PREFIX}/odata/Category?$count=true`
    );
  }
}
