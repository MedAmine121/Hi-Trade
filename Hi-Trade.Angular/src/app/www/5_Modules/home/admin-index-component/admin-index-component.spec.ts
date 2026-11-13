import { describe, it, expect, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideToastr } from 'ngx-toastr';
import { provideNgxStripe } from 'ngx-stripe';
import { of } from 'rxjs';
import { AdminIndexComponent } from './admin-index-component';
import { AdminBLLService } from '../../../4_BLL/admin-bll.service';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';

describe('AdminIndexComponent', () => {
  let component: AdminIndexComponent;
  let fixture: ComponentFixture<AdminIndexComponent>;
  let mockAdminBLL: Partial<AdminBLLService>;
  let mockAssetBLL: Partial<AssetBLLService>;

  beforeEach(async () => {
    mockAdminBLL = {
      getDashboardStats$: () =>
        of({
          totalUsers: 10,
          totalTraders: 9,
          totalAdmins: 1,
          totalPortfolios: 5,
          totalAssets: 8,
          enabledAssets: 6,
          disabledAssets: 2,
          totalTransactions: 20,
          buyTransactionsCount: 12,
          sellTransactionsCount: 8,
          totalTradingVolume: 45000,
          totalUserBalances: 15000,
          totalDepositedAmount: 20000,
          recentTransactions: [],
          recentUsers: [],
        }),
      getUsers$: () => of([]),
      getTransactions$: () => of([]),
      toggleUserStatus$: () => of({ success: true, message: 'Status updated' }),
    };

    mockAssetBLL = {
      getAllAssets$: () =>
        of([
          { id: 1, ticker: 'AAPL', name: 'Apple Inc.', currentPrice: 150, enabled: true },
          { id: 2, ticker: 'TSLA', name: 'Tesla Inc.', currentPrice: 200, enabled: false },
        ]),
      toggleAssetStatus$: () => of({ success: true, message: 'Asset status updated' }),
    };

    await TestBed.configureTestingModule({
      imports: [AdminIndexComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        provideToastr(),
        provideNgxStripe(),
        { provide: AdminBLLService, useValue: mockAdminBLL },
        { provide: AssetBLLService, useValue: mockAssetBLL },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminIndexComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create backoffice dashboard component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with overview tab', () => {
    expect(component.activeTab()).toBe('overview');
  });

  it('should switch tabs properly', () => {
    component.setActiveTab('users');
    expect(component.activeTab()).toBe('users');

    component.setActiveTab('transactions');
    expect(component.activeTab()).toBe('transactions');

    component.setActiveTab('assets');
    expect(component.activeTab()).toBe('assets');
  });

  it('should load dashboard stats on init', () => {
    component.loadDashboardStats();
    expect(component.dashboardStats()).toBeTruthy();
    expect(component.dashboardStats()?.totalUsers).toBe(10);
    expect(component.dashboardStats()?.totalTradingVolume).toBe(45000);
  });

  it('should calculate enabled and disabled assets counts', () => {
    component.loadAssets();
    expect(component.totalCount()).toBe(2);
    expect(component.enabledCount()).toBe(1);
    expect(component.disabledCount()).toBe(1);
  });
});
