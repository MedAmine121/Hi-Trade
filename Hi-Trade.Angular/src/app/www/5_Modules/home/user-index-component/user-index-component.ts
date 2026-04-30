import { Component, OnInit, Signal, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";
import { Context } from '../../../2_Models/responses/context.model';
import { PortfolioDTO } from '../../../2_Models/responses/portfolio.model';
import { BaseComponent } from '../../shared/base-component/base-component';
import { PortfolioBLLService } from '../../../4_BLL/portfolio-bll.service';

@Component({
  selector: 'app-user-index-component',
  imports: [NavBarComponent, CommonModule],
  templateUrl: './user-index-component.html',
  styleUrl: './user-index-component.css'
})
export class UserIndexComponent extends BaseComponent implements OnInit {
  private portfolioBLLService = inject(PortfolioBLLService);
  
  userContext: Context | null = null;
  portfolios: PortfolioDTO[] = [];
  isLoading = signal(false);
  error: string | null = null;

  ngOnInit(): void {
    this.loadUserContext();
    this.loadPortfolios();
  }

  private loadUserContext(): void {
    const context = this.storageService.getLocalStorage('context');
    if (context) {
      this.userContext = context as Context;
    }
  }

  private loadPortfolios(): void {
    this.isLoading.set(true);
    this.error = null;
    
    this.portfolioBLLService.getPortfolios$().subscribe({
      next: (portfolios) => {
        if (portfolios) {
          this.portfolios = portfolios;
        } else {
          this.error = 'No portfolios available';
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error = 'Failed to load portfolios';
        this.notificationService.showErrorToast('Error loading portfolios');
        this.isLoading.set(false);
      }
    });
  }

  getPositionValue(quantity: number, averagePrice: number): number {
    return quantity * averagePrice;
  }

  addPortfolio(): void {
    // Navigate to add portfolio page or open modal
    // Placeholder for now - adjust route as needed
    this.notificationService.showSuccessToast('Add Portfolio feature coming soon');
    // Example navigation: this.redirectTo(['/add-portfolio']);
  }
}
