import { Component, OnInit, Signal, ViewChild, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";
import { AddPortfolioModalComponent } from '../../shared/add-portfolio-modal/add-portfolio-modal.component';
import { BuyAssetModalComponent } from '../../shared/buy-asset-modal-component/buy-asset-modal-component';
import { Context } from '../../../2_Models/responses/context.model';
import { PortfolioDTO } from '../../../2_Models/responses/portfolio.model';
import { AssetDTO } from '../../../2_Models/responses/asset.model';
import { CreatePortfolioRequest } from '../../../2_Models/requests/create-portfolio-request.model';
import { BuyAssetRequest } from '../../../2_Models/requests/buy-asset-request.model';
import { BaseComponent } from '../../shared/base-component/base-component';
import { PortfolioBLLService } from '../../../4_BLL/portfolio-bll.service';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';
import {MatTabChangeEvent, MatTabsModule} from '@angular/material/tabs';

@Component({
  selector: 'app-user-index-component',
  imports: [NavBarComponent, CommonModule, AddPortfolioModalComponent, BuyAssetModalComponent, MatTabsModule],
  templateUrl: './user-index-component.html',
  styleUrl: './user-index-component.css'
})
export class UserIndexComponent extends BaseComponent implements OnInit {
  @ViewChild(AddPortfolioModalComponent) addPortfolioModal!: AddPortfolioModalComponent;
  @ViewChild(BuyAssetModalComponent) buyAssetModal!: BuyAssetModalComponent;
  
  private portfolioBLLService = inject(PortfolioBLLService);
  private assetBLLService = inject(AssetBLLService);
  
  userContext: Context | null = null;
  portfolios = signal<PortfolioDTO[]>([]);
  assets: AssetDTO[] = [];
  isLoading = signal(false);
  selectedPortfolioIndex = signal(0);
  selectedPortfolio = computed(() => this.portfolios()[this.selectedPortfolioIndex()]);

  ngOnInit(): void {
    this.loadUserContext();
    this.loadPortfolios();
    this.loadAssets();
  }

  private loadUserContext(): void {
    const context = this.storageService.getLocalStorage('context');
    if (context) {
      this.userContext = context as Context;
    }
  }

  private loadPortfolios(): void {
    this.isLoading.set(true);
    
    this.portfolioBLLService.getPortfolios$().subscribe({
      next: (portfolios) => {
        if (portfolios) {
          this.portfolios.set(portfolios);
          this.selectedPortfolio();
        } else {
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.notificationService.showErrorToast('Error loading portfolios');
        this.isLoading.set(false);
      }
    });
  }

  private loadAssets(): void {
    this.assetBLLService.getAssets$().subscribe({
      next: (assets) => {
        if (assets) {
          this.assets = assets;
        }
      },
      error: (err) => {
        this.notificationService.showErrorToast('Error loading assets');
      }
    });
  }

  getPositionValue(quantity: number, averagePrice: number): number {
    return quantity * averagePrice;
  }

  addPortfolio(): void {
    this.addPortfolioModal.openModal();
  }

  onPortfolioCreated(portfolioName: string): void {
    const request: CreatePortfolioRequest = {
      name: portfolioName
    };

    this.isLoading.set(true);
    this.portfolioBLLService.createPortfolio$(request).subscribe({
      next: (response) => {
        if (response?.success) {
          this.notificationService.showSuccessToast('Portfolio created successfully');
          this.loadPortfolios();
        } else {
          this.notificationService.showErrorToast(response?.message ?? '');
          this.isLoading.set(false);
        }
      },
      error: (err) => {
        this.notificationService.showErrorToast('Failed to create portfolio');
        this.isLoading.set(false);
      }
    });
  }

  buyAsset(): void {
    this.buyAssetModal.openModal();
  }

  onAssetBuy(data: { asset: AssetDTO; quantity: number }): void {
    const selectedPortfolio = this.selectedPortfolio();
    if (!selectedPortfolio) {
      this.notificationService.showErrorToast('Please select a portfolio');
      return;
    }

    const request: BuyAssetRequest = {
      portfolioId: selectedPortfolio.id,
      assetId: data.asset.id,
      quantity: data.quantity
    };

    this.isLoading.set(true);
    this.portfolioBLLService.buyAsset$(request).subscribe({
      next: (response) => {
        if (response?.success) {
          this.notificationService.showSuccessToast('Asset purchased successfully');
          this.loadPortfolios();
        } else {
          this.notificationService.showErrorToast(response?.message ?? 'Failed to buy asset');
          this.isLoading.set(false);
        }
      },
      error: (err) => {
        this.notificationService.showErrorToast('Failed to buy asset');
        this.isLoading.set(false);
      }
    });
  }

  selectedPortfolioChanged(event: MatTabChangeEvent): void{
    this.selectedPortfolioIndex.set(event.index);
  }
}
