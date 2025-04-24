import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';

interface Vault {
  label: string;
  amount: number;
  currency: string;
  icon: string;
}

@Component({
  selector: 'app-vaults',
  standalone: true,
  imports: [CommonModule, CandleComponent, SweepButtonComponent],
  templateUrl: './vaults.component.html',
  styleUrls: ['./vaults.component.scss'],
})
export class VaultsComponent {
  // Display the current user's key name (replace with real data)
  keyName = 'Key Name';

  // Decorative candles at top
  candles = Array.from({ length: 5 }).map(() => ({
    height: `${40 + Math.random() * 20}px`,
    flameWidth: `${10 + Math.random() * 5}px`,
    flameHeight: `${18 + Math.random() * 6}px`,
    delay: `${Math.random() * 1.5}s`,
    flameDelay: `${Math.random() * 2}s`,
  }));

  // Vault balances
  vaults: Vault[] = [
    { label: 'Galleons',       amount: 1250, currency: 'Galleons',    icon: '🟡' },
    { label: 'Sickles',        amount: 540,  currency: 'Sickles',     icon: '⚪️' },
    { label: 'Knuts',          amount: 7200, currency: 'Knuts',       icon: '🟤' },
    { label: 'Muggle Money',   amount: 3000, currency: 'DKK',         icon: '💳' }
  ];

  constructor(private router: Router) {}

  /** Log the user out */
  logout() {
    // adjust path if needed
    this.router.navigate(['/users/login']);
  }

  /** Navigate to account details */
  goToAccount() {
    this.router.navigate(['/account-details']);
  }

  /** Navigate to conversion tool */
  goToConversion() {
    this.router.navigate(['/convert']);
  }

  /** Handle vault card click */
  selectVault(v: Vault) {
    console.log('Vault selected:', v);
    // e.g. this.router.navigate(['/vaults', v.id]);
  }
}
