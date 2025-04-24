import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './account-details.component.html',
  styleUrls: ['./account-details.component.scss'],
})
export class AccountDetailsComponent {
  user = {
    name: 'Harry Potter',
    email: 'hp@hogwarts.edu',
    joined: 'June 1, 1991'
  };
}
