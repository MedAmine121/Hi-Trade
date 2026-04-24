import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavBarComponent } from "../shared/nav-bar-component/nav-bar-component";

@Component({
  selector: 'app-landing-page',
  imports: [NavBarComponent],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPageComponent {}
