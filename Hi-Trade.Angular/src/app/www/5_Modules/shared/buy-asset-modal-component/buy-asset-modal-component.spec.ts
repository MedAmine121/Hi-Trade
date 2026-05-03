import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyAssetModalComponent } from './buy-asset-modal-component';

describe('BuyAssetModalComponent', () => {
  let component: BuyAssetModalComponent;
  let fixture: ComponentFixture<BuyAssetModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BuyAssetModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BuyAssetModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
