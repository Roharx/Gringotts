import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sweep-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sweep-button.component.html',
  styleUrls: ['./sweep-button.component.scss'],
})
export class SweepButtonComponent {
  /** Emitted when the user clicks the button */
  @Output() clicked = new EventEmitter<void>();
}
