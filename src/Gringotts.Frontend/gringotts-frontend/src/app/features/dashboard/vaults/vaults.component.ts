import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';
import { ConversionModalComponent } from '../modals/conversion-modal/conversion-modal.component';

interface Vault {
  label: string;
  amount: number;
  currency: string;
  icon: string;
}

@Component({
  selector: 'app-vaults',
  standalone: true,
  imports: [CommonModule, CandleComponent, SweepButtonComponent, ConversionModalComponent],
  templateUrl: './vaults.component.html',
  styleUrls: ['./vaults.component.scss'],
})
export class VaultsComponent {
  keyName = 'YourKeyName';

  candles = Array.from({ length: 5 }).map(() => ({
    height: `${40 + Math.random() * 20}px`,
    flameWidth: `${10 + Math.random() * 5}px`,
    flameHeight: `${18 + Math.random() * 6}px`,
    delay: `${Math.random() * 1.5}s`,
    flameDelay: `${Math.random() * 2}s`,
  }));

  vaults: Vault[] = [
    { label: 'Galleons',     amount: 1250, currency: 'Galleons',  icon: '🟡' },
    { label: 'Sickles',      amount: 540,  currency: 'Sickles',   icon: '⚪️' },
    { label: 'Knuts',        amount: 7200, currency: 'Knuts',     icon: '🟤' },
    { label: 'Muggle Money', amount: 3000, currency: 'DKK',       icon: '💳' }
  ];

  conversionVault: Vault | null = null;

  constructor(private router: Router) {}

  logout() { this.router.navigate(['/users/login']); }
  goToAccount() { this.router.navigate(['/account-details']); }
  goToTransfer() { this.router.navigate(['/transfer']); }

  openConversion(v: Vault) {
    this.conversionVault = v;
  }

  closeConversion() {
    this.conversionVault = null;
  }

  selectVault(v: Vault) {
    console.log('Vault selected:', v);
  }
}
