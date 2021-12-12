import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  loggedIn: boolean;

  constructor(public service: AccountService) { }

  ngOnInit(): void {
  }

  login() {
    this.service.login(this.model).subscribe( response => {
      this.loggedIn = true;
    }, error => {
      console.log(error);
    });
  }

  logout() {
    this.service.logout();
    this.loggedIn = false;
  }
}
