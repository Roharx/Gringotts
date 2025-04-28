import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Vault {
  label: string;
  amount: number;
  currency: string;
  icon: string;
}

@Component({
  selector: 'app-conversion-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './conversion-modal.component.html',
  styleUrls: ['./conversion-modal.component.scss'],
})
export class ConversionModalComponent {
  private _vault!: Vault;

  /** When a vault is set, initialize target to the first valid option */
  @Input()
  set vault(v: Vault) {
    this._vault = v;
    this.target = this.targetOptions[0];
  }
  get vault() {
    return this._vault;
  }

  @Output() close = new EventEmitter<void>();

  amount = 0;
  target = '';
  result: number | null = null;

  // all possible currencies
  currencies = ['Galleons', 'Sickles', 'Knuts', 'DKK'];

  // show only currencies different from the source
  get targetOptions() {
    return this.currencies.filter(c => c !== this.vault.currency);
  }

  // example rates
  rates: Record<string, number> = {
    'Galleons->Sickles': 17,
    'Galleons->Knuts': 493,
    'Galleons->DKK': 100,
    'Sickles->Galleons': 1/17,
    'Sickles->Knuts': 29,
    'Sickles->DKK': 5.5,
    'Knuts->Galleons': 1/493,
    'Knuts->Sickles': 1/29,
    'Knuts->DKK': 0.19,
    'DKK->Galleons': 1/100,
    'DKK->Sickles': 1/5.5,
    'DKK->Knuts': 5.2
  };

  doConvert() {
    const key = `${this.vault.currency}->${this.target}`;
    this.result = this.amount * (this.rates[key] ?? 1);
  }

  onClose() {
    this.close.emit();
  }

  /** Selects the entire input content on focus */
  selectAll(event: FocusEvent) {
    const input = event.target as HTMLInputElement;
    input.select();
  }
}
