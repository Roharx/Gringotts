import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CandleComponent } from '../../../shared/styling/candle/candle.component';
import { SweepButtonComponent } from '../../../shared/styling/sweep-button/sweep-button.component';
import { ApiService } from '../../../shared/services/api.service';
import { TokenService } from '../../../shared/services/token.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CandleComponent,
    SweepButtonComponent
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  username = '';
  password = '';
  error: string | null = null;

  candles = Array.from({ length: 5 }).map(() => ({
    height: `${40 + Math.floor(Math.random() * 15)}px`,
    flameWidth: `${10 + Math.floor(Math.random() * 3)}px`,
    flameHeight: `${18 + Math.floor(Math.random() * 4)}px`,
    delay: `${Math.random() * 1.5 + 0.3}s`,
    flameDelay: `${Math.random() * 2}s`,
  }));

  constructor(
    private api: ApiService,
    private tokenService: TokenService,
    private router: Router
  ) {}

  login() {
    this.error = null;
    this.api.post<{ token: string }>('users/login', {
      username: this.username,
      password: this.password
    }).subscribe({
      next: res => {
        this.tokenService.setToken(res.token);
        this.router.navigate(['/vaults']);
      },
      error: err => {
        if (err.status === 401) {
          this.error = 'Invalid username or password';
        } else {
          this.error = 'Failed to login';
        }
      }
    });
  }


  goToRegister() {
    this.router.navigate(['/users/register']);
  }
}
