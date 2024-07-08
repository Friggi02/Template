import { Component, HostListener } from '@angular/core';
import { Content, sideBarData } from '../../app-routing.module';

@Component({
  selector: 'app-app-shell',
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss']
})
export class AppShellComponent {
  
  screenWidth: number = 0;
  navData: Content[] = sideBarData;
  collapsed: boolean = false;
  visible: boolean = true;

  constructor() {
    this.onResize();
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    this.screenWidth = window.innerWidth;
    this.updateVisibility();
  }

  toggleCollapse() {
    this.collapsed = !this.collapsed;
    this.updateVisibility();
  }

  updateVisibility() {
    this.visible = !(this.screenWidth < 600 && !this.collapsed);
  }
  
  get contentClass() {
    return this.collapsed ? 'content-collapsed' : 'content-expanded';
  }
}