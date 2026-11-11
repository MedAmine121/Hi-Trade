import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { UserService } from '../../../1_Services/user.service';
import { AssetDTO } from '../../../2_Models/responses/asset.model';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';
import { BaseComponent } from '../../shared/base-component/base-component';
import { CreateAssetModalComponent } from '../../shared/create-asset-modal-component/create-asset-modal-component';
import { NavBarComponent } from '../../shared/nav-bar-component/nav-bar-component';

type AssetStatusFilter = 'all' | 'enabled' | 'disabled';

@Component({
  selector: 'app-admin-index-component',
  imports: [CommonModule, FormsModule, NavBarComponent, CreateAssetModalComponent],
  templateUrl: './admin-index-component.html',
  styleUrl: './admin-index-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminIndexComponent extends BaseComponent implements OnInit {
  private assetBLLService = inject(AssetBLLService);
  private userService = inject(UserService);
  private destroyRef = inject(DestroyRef);

  private readonly searchInput$ = new Subject<string>();
  private readonly reloadTrigger$ = new Subject<void>();

  @ViewChild(CreateAssetModalComponent) createAssetModal!: CreateAssetModalComponent;

  assets = signal<AssetDTO[]>([]);
  isLoading = signal<boolean>(false);
  togglingAssetId = signal<number | null>(null);
  searchQuery = signal<string>('');
  statusFilter = signal<AssetStatusFilter>('all');

  totalCount = computed(() => this.assets().length);
  enabledCount = computed(() => this.assets().filter((asset) => asset.enabled).length);
  disabledCount = computed(() => this.assets().filter((asset) => !asset.enabled).length);
  hasActiveFilters = computed(
    () => this.searchQuery().trim() !== '' || this.statusFilter() !== 'all',
  );

  ngOnInit(): void {
    this.userService.fetchUser();

    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadAssets());

    this.reloadTrigger$
      .pipe(
        tap(() => this.isLoading.set(true)),
        switchMap(() => {
          const enabledParam = this.resolveEnabledFilter(this.statusFilter());
          return this.assetBLLService
            .getAllAssets$(this.searchQuery(), enabledParam)
            .pipe(catchError(() => of(null)));
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        this.isLoading.set(false);
        if (data) {
          this.assets.set(data);
        }
      });

    this.loadAssets();
  }

  loadAssets(): void {
    this.reloadTrigger$.next();
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
    this.searchInput$.next(value);
  }

  onStatusFilterChange(filter: AssetStatusFilter): void {
    this.statusFilter.set(filter);
    this.loadAssets();
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.statusFilter.set('all');
    this.loadAssets();
  }

  openCreateAssetModal(): void {
    this.createAssetModal.openModal();
  }

  onAssetCreated(): void {
    this.loadAssets();
  }

  toggleAssetStatus(asset: AssetDTO): void {
    if (this.togglingAssetId() !== null) {
      return;
    }

    this.togglingAssetId.set(asset.id);
    this.assetBLLService
      .toggleAssetStatus$(asset.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.togglingAssetId.set(null)),
      )
      .subscribe((response) => {
        if (!response) {
          return;
        }
        if (response.success) {
          this.notificationService.showSuccessToast(
            response.message || `Asset ${asset.ticker} status updated.`,
          );
          this.loadAssets();
        } else {
          this.notificationService.showErrorToast(
            response.message || 'Failed to update asset status.',
          );
        }
      });
  }

  private resolveEnabledFilter(filter: AssetStatusFilter): boolean | undefined {
    if (filter === 'enabled') {
      return true;
    }
    if (filter === 'disabled') {
      return false;
    }
    return undefined;
  }
}
