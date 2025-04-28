import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';
import { ConversionModalComponent } from '../modals/conversion-modal/conversion-modal.component';
import { ApiService } from '../../../shared/services/api.service';
import { TokenService } from '../../../shared/services/token.service';

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
}
