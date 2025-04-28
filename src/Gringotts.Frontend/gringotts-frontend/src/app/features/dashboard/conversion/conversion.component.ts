import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-conversion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './conversion.component.html',
  styleUrls: ['./conversion.component.scss'],
})
export class ConversionComponent {
  amount = 0;
  from = 'Galleons';
  to = 'Sickles';
  result = 0;

  convert() {
    // placeholder conversion logic
    this.result = this.amount * 17; // e.g., 1 Galleon = 17 Sickles
  }
}
