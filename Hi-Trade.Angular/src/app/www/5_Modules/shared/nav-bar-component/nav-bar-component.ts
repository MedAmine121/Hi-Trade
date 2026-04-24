import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-bar-component',
  imports: [],
  templateUrl: './nav-bar-component.html',
  styleUrl: './nav-bar-component.css',
})
export class NavBarComponent {
  private router = inject(Router);
  redirectTo(location: string): void{
    this.router.navigate([location]);
  }
}
