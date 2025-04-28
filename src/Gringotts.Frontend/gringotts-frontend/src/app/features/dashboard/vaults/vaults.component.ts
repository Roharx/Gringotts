import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';
import { ConversionModalComponent } from '../modals/conversion-modal/conversion-modal.component';
import { ApiService } from '../../../shared/services/api.service';
import { TokenService } from '../../../shared/services/token.service';
import { FormsModule } from '@angular/forms';

interface Vault {
  label:    string;
  amount:   number;
  currency: string;
  icon:     string;
}

interface Balance {
  id:        string;
  userId:    string;
  dkkAmount: number;
  galleons:  number;
  sickles:   number;
  knuts:     number;
  updatedAt: string;
}

@Component({
  selector: 'app-vaults',
  standalone: true,
  imports: [
    CommonModule,
    CandleComponent,
    FormsModule,
    SweepButtonComponent,
    ConversionModalComponent
  ],
  templateUrl: './vaults.component.html',
  styleUrls: ['./vaults.component.scss'],
})
export class VaultsComponent implements OnInit {
  keyName = '';
  candles = Array.from({ length: 5 }).map(() => ({
    height:     `${40 + Math.random() * 20}px`,
    flameWidth: `${10 + Math.random() * 5}px`,
    flameHeight:`${18 + Math.random() * 6}px`,
    delay:      `${Math.random() * 1.5}s`,
    flameDelay: `${Math.random() * 2}s`,
  }));

  vaults: Vault[] = [];
  conversionVault: Vault | null = null;

  constructor(
    private api:          ApiService,
    private tokenService: TokenService,
    private router:       Router
  ) {}

  ngOnInit() {
    const payload = this.tokenService.getPayload();
    const userId  = payload?.sub || payload?.username;
    this.keyName  = payload?.displayName || userId || '';

    if (!userId) { return; }

    this.api.get<Balance>(`balance/${userId}`)
      .subscribe({
        next: bal => {
          this.vaults = [
            { label: 'Galleons',     amount: bal.galleons,  currency: 'Galleons',  icon: '🟡' },
            { label: 'Sickles',      amount: bal.sickles,   currency: 'Sickles',   icon: '⚪️' },
            { label: 'Knuts',        amount: bal.knuts,     currency: 'Knuts',     icon: '🟤' },
            { label: 'Muggle Money', amount: bal.dkkAmount, currency: 'DKK',       icon: '💳' }
          ];
        },
        error: err => {
          console.error('Failed to load balances', err);
          this.vaults = [
            { label: 'Galleons',     amount: 0, currency: 'Galleons',  icon: '🟡' },
            { label: 'Sickles',      amount: 0, currency: 'Sickles',   icon: '⚪️' },
            { label: 'Knuts',        amount: 0, currency: 'Knuts',     icon: '🟤' },
            { label: 'Muggle Money', amount: 0, currency: 'DKK',       icon: '💳' }
          ];
        }
      });
  }

  logout() {
    this.tokenService.removeToken();
    this.router.navigate(['/users/login']);
  }

  goToAccount()   { this.router.navigate(['/account-details']); }
  goToTransfer()  { this.router.navigate(['/transfer']); }
  openConversion(v: Vault) { this.conversionVault = v; }
  closeConversion()        { this.conversionVault = null; }


  // placeholder for demo:
  giveMyselfMoney(currency: string) {
    const payload = this.tokenService.getPayload();
    const userId = payload?.sub || payload?.username;

    if (!userId) {
      console.error('No user ID found.');
      return;
    }

    const amount = prompt(`How much ${currency} do you want to give yourself?`);

    if (!amount || isNaN(Number(amount)) || Number(amount) <= 0) {
      alert('Please enter a valid positive number.');
      return;
    }

    const numAmount = Number(amount);

    const money = {
      galleons: 0,
      sickles: 0,
      knuts: 0
    };
    let dkkAmount = 0;

    if (currency === 'Galleons') money.galleons = numAmount;
    else if (currency === 'Sickles') money.sickles = numAmount;
    else if (currency === 'Knuts') money.knuts = numAmount;
    else if (currency === 'DKK') dkkAmount = numAmount;
    else {
      alert('Unsupported currency.');
      return;
    }

    this.api.post<any>('transactions', {
      userId: userId,
      type: 0,                  // 0 = Credit (add funds)
      amount: money,
      dkkAmount: dkkAmount,
      description: 'Self-granted reward'
    }).subscribe({
      next: () => {
        alert(`Successfully added ${amount} ${currency}!`);
        this.ngOnInit();
      },
      error: (err) => {
        console.error('Failed to add money', err);
        alert('Failed to add money.');
      }
    });
  }

  adminFeatureToggles: Record<string, boolean> = {
    login: true,
    register: true,
    transaction: true,
    conversion: true,
    recurring: true,
    exchange: true
  };
  toggleFeature(feature: string) {
    const enabled = this.adminFeatureToggles[feature];
    const url = `http://161.97.92.174:5000/api/admin/toggle-feature?feature=${feature}&enabled=${enabled}`;

    fetch(url, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.tokenService.getToken()}`,
        'Content-Type': 'application/json'
      },
      body: '{}' // API expects an empty body
    })
      .then(response => {
        if (!response.ok) {
          throw new Error('Failed to toggle feature');
        }
        return response.json();
      })
      .then(data => {
        alert(`Feature '${feature}' has been ${enabled ? 'enabled' : 'disabled'}.`);
        console.log('Toggle response:', data);
      })
      .catch(err => {
        console.error('Failed to toggle feature', err);
        alert('Failed to toggle feature.');
      });
  }
  adminFeatureToggleKeys() {
    return Object.keys(this.adminFeatureToggles);
  }
}
