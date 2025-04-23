import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-candle',
  standalone: true,
  imports: [CommonModule, DragDropModule],
  templateUrl: './candle.component.html',
  styleUrls: ['./candle.component.scss'],
})
export class CandleComponent {
  @Input() height = '50px';
  @Input() flameWidth = '12px';
  @Input() flameHeight = '20px';
  @Input() delay = '0s';
  @Input() flameDelay = '0s';
  @Input() initialX = 0;
  @Input() initialY = 0;
}
