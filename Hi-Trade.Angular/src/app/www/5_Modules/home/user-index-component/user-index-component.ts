import { Component, OnInit, Signal, ViewChild, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";
import { AddPortfolioModalComponent } from '../../shared/add-portfolio-modal/add-portfolio-modal.component';
import { BuyAssetModalComponent } from '../../shared/buy-asset-modal-component/buy-asset-modal-component';
import { SellAssetModalComponent } from '../../shared/sell-asset-modal-component/sell-asset-modal-component';
import { Context } from '../../../2_Models/responses/context.model';
import { PortfolioDTO } from '../../../2_Models/responses/portfolio.model';
import { PositionDTO } from '../../../2_Models/responses/position.model';
import { AssetDTO } from '../../../2_Models/responses/asset.model';
import { CreatePortfolioRequest } from '../../../2_Models/requests/create-portfolio-request.model';
import { BuyAssetRequest } from '../../../2_Models/requests/buy-asset-request.model';
import { SellAssetRequest } from '../../../2_Models/requests/sell-asset-request.model';
import { BaseComponent } from '../../shared/base-component/base-component';
import { PortfolioBLLService } from '../../../4_BLL/portfolio-bll.service';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';
import {MatTabChangeEvent, MatTabsModule} from '@angular/material/tabs';
import { UserService } from '../../../1_Services/user.service';
import { StripeElementsOptions } from '@stripe/stripe-js';
import { AddFundsComponent } from "../../shared/add-funds-component/add-funds-component";
import { TransactionHistoryComponent } from "../../shared/transaction-history-component/transaction-history-component";
import { TransactionDTO } from '../../../2_Models/responses/transaction.model';
import { VerifyEmailModalComponent } from '../../shared/verify-email-modal-component/verify-email-modal.component';
import { UserBLLService } from '../../../4_BLL/user-bll.service';

@Component({
  selector: 'app-user-index-component',
  imports: [NavBarComponent, CommonModule, AddPortfolioModalComponent, BuyAssetModalComponent, SellAssetModalComponent, MatTabsModule, TransactionHistoryComponent, VerifyEmailModalComponent],
  templateUrl: './user-index-component.html',
  styleUrl: './user-index-component.css'
})
export class UserIndexComponent extends BaseComponent implements OnInit {
  @ViewChild(AddPortfolioModalComponent) addPortfolioModal!: AddPortfolioModalComponent;
  @ViewChild(BuyAssetModalComponent) buyAssetModal!: BuyAssetModalComponent;
  @ViewChild(SellAssetModalComponent) sellAssetModal!: SellAssetModalComponent;
  @ViewChild(AddFundsComponent) addFundsModal!: AddFundsComponent;
  @ViewChild(TransactionHistoryComponent) transactionHistoryModal!: TransactionHistoryComponent;
  @ViewChild(VerifyEmailModalComponent) verifyEmailModal!: VerifyEmailModalComponent;

  private portfolioBLLService = inject(PortfolioBLLService);
  private assetBLLService = inject(AssetBLLService);
  private userService = inject(UserService);
  private userBLLService = inject(UserBLLService);
  elementsOptions: StripeElementsOptions = {
    locale: 'en',
    appearance: {
      theme: 'stripe',
      labels: 'floating',
      variables: {
        colorPrimary: '#673ab7',
      },
    },
  };
  userContext: Context | null = null;
  portfolios = signal<PortfolioDTO[]>([]);
  assets = signal<AssetDTO[]>([]);
  isLoading = signal(false);
  selectedPortfolioIndex = signal(0);
  selectedPortfolio = computed(() => this.portfolios()[this.selectedPortfolioIndex()]);
  transactions = signal<TransactionDTO[]>([]);
  isResendingVerification = signal(false);

  isEmailVerified = computed(() => {
    const user = this.userService.user();
    if (user !== null && user !== undefined) {
      return !!user.isEmailVerified;
    }
    return !!this.userContext?.isEmailVerified;
  });

  ngOnInit(): void {
    this.loadUserContext();
    this.loadPortfolios();
    this.loadAssets();
    this.userService.fetchUser();
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
      next: (assets: AssetDTO[] | null) => {
        if (assets) {
          this.assets.set(assets);
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
    if (!this.isEmailVerified()) {
      this.notificationService.showWarningToast('Please verify your email address to buy assets.');
      this.openVerifyEmailModal();
      return;
    }
    this.loadAssets();
    this.buyAssetModal.openModal();
  }
  viewTransactions(): void {
    this.portfolioBLLService.getTransactions$(this.selectedPortfolio() ? { portfolioId: this.selectedPortfolio()!.id } : { portfolioId: 0 }).subscribe({
      next: (transactions: TransactionDTO[] | null) => {
        if (transactions) {
          this.transactions.set(transactions);
          this.transactionHistoryModal.openModal();
        }
      },
      error: (err) => {
        this.notificationService.showErrorToast('Error loading transactions');
      }
    });
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
          this.userService.fetchUser();
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

  sellAsset(position: PositionDTO): void {
    if (!this.isEmailVerified()) {
      this.notificationService.showWarningToast('Please verify your email address to sell assets.');
      this.openVerifyEmailModal();
      return;
    }
    this.sellAssetModal.openModal(position);
  }

  openVerifyEmailModal(codeAlreadySent: boolean = false): void {
    const email = this.userService.user()?.email ?? this.userContext?.email;
    this.verifyEmailModal.openModal(email, codeAlreadySent);
  }

  resendVerificationFromBanner(): void {
    const email = this.userService.user()?.email ?? this.userContext?.email;
    if (!email) {
      this.notificationService.showErrorToast('User email not found.');
      return;
    }
    this.isResendingVerification.set(true);
    this.userBLLService.resendVerification$({ email }).subscribe({
      next: (response) => {
        this.isResendingVerification.set(false);
        if (response?.success) {
          this.notificationService.showSuccessToast('Verification code sent successfully. Please check your inbox.');
          this.openVerifyEmailModal(true);
        } else {
          this.notificationService.showErrorToast(response?.message ?? 'Failed to send verification email.');
        }
      },
      error: () => {
        this.isResendingVerification.set(false);
        this.notificationService.showErrorToast('Failed to send verification email.');
      }
    });
  }

  onEmailVerified(): void {
    const current = this.userService.user();
    if (current) {
      this.userService.user.set({ ...current, isEmailVerified: true });
    }
    const ctx = this.storageService.getLocalStorage('context') as Context | null;
    if (ctx) {
      ctx.isEmailVerified = true;
      this.storageService.setLocalStorage('context', ctx);
      this.userContext = ctx;
    }
    this.notificationService.showSuccessToast('Email verified successfully! Trading actions are now unlocked.');
  }

  onAssetSell(data: { position: PositionDTO; quantity: number }): void {
    const selectedPortfolio = this.selectedPortfolio();
    if (!selectedPortfolio) {
      this.notificationService.showErrorToast('Please select a portfolio');
      return;
    }

    const request: SellAssetRequest = {
      portfolioId: selectedPortfolio.id,
      positionId: data.position.id,
      quantity: data.quantity
    };

    this.isLoading.set(true);
    this.portfolioBLLService.sellAsset$(request).subscribe({
      next: (response) => {
        if (response?.success) {
          this.notificationService.showSuccessToast('Asset sold successfully');
          this.loadPortfolios();
          this.userService.fetchUser();
        } else {
          this.notificationService.showErrorToast(response?.message ?? 'Failed to sell asset');
          this.isLoading.set(false);
        }
      },
      error: (err) => {
        this.notificationService.showErrorToast('Failed to sell asset');
        this.isLoading.set(false);
      }
    });
  }
}
