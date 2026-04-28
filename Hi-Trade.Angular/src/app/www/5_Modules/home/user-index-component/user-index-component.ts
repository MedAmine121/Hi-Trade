import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";
import { Context } from '../../../2_Models/responses/context.model';
import { BaseComponent } from '../../shared/base-component/base-component';

@Component({
  selector: 'app-user-index-component',
  imports: [NavBarComponent, CommonModule],
  templateUrl: './user-index-component.html',
  styleUrl: './user-index-component.css'
})
export class UserIndexComponent extends BaseComponent implements OnInit {
  userContext: Context | null = null;


  ngOnInit(): void {
    this.loadUserContext();
  }

  loadUserContext(): void {
    const context = this.storageService.getLocalStorage('context');
    if (context) {
      this.userContext = context as Context;
    }
  }
}
