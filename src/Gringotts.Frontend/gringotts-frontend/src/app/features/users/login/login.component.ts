import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CandleComponent } from '../../../shared/candle/candle.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CandleComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  username = '';
  password = '';

  candles = Array.from({ length: 5 }).map(() => ({
    height: `${40 + Math.floor(Math.random() * 15)}px`,
    flameWidth: `${10 + Math.floor(Math.random() * 3)}px`,
    flameHeight: `${18 + Math.floor(Math.random() * 4)}px`,
    delay: `${Math.random() * 1.5 + 0.3}s`,
    flameDelay: `${Math.random() * 2}s`,
  }));

  constructor(private router: Router) {}

  login() {
    console.log('Logging in with', this.username, this.password);
  }

  goToRegister() {
    this.router.navigate(['/users/register']);
  }
}
