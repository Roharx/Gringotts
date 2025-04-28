import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';
import { ApiService } from '../../../shared/services/api.service';

type Candle = {
  height: string;
  flameWidth: string;
  flameHeight: string;
  delay: string;
  flameDelay: string;
  initialX: number;
  initialY: number;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CandleComponent,
    SweepButtonComponent
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  username = '';
  email = '';
  displayName = '';
  password = '';
  error: string | null = null;

  candles: Candle[] = [];

  constructor(
    private api: ApiService,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    const formZone = { top: 25, bottom: 75, left: 30, right: 70 };
    let tries = 0;
    while (this.candles.length < 12 && tries < 200) {
      tries++;
      const topVH = Math.random() * 90;
      const leftVW = Math.random() * 90;
      const overForm =
        topVH > formZone.top &&
        topVH < formZone.bottom &&
        leftVW > formZone.left &&
        leftVW < formZone.right;
      if (overForm) continue;
      const initialX = (leftVW / 100) * window.innerWidth;
      const initialY = (topVH / 100) * window.innerHeight;
      this.candles.push({
        height: `${40 + Math.floor(Math.random() * 15)}px`,
        flameWidth: `${10 + Math.floor(Math.random() * 3)}px`,
        flameHeight: `${18 + Math.floor(Math.random() * 4)}px`,
        delay: `${Math.random() * 1.5 + 0.3}s`,
        flameDelay: `${Math.random() * 2}s`,
        initialX,
        initialY,
      });
    }
  }

  goBack(): void {
    this.location.back();
  }

  register() {
    this.error = null;
    this.api.post('users/register', {
      username: this.username,
      email: this.email,
      displayName: this.displayName,
      password: this.password
    }).subscribe({
      next: () => this.router.navigate(['/users/login']),
      error: () => this.error = 'Registration failed. Please try again.'
    });
  }
}
