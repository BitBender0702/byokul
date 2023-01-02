import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SchoolService } from 'src/root/service/school.service';

@Component({
    selector: 'post-view',
    templateUrl: './postView.component.html',
    styleUrls: ['./postView.component.css']
  })

export class PostViewComponent implements OnInit {

    // isOpenSidebar:boolean = false;

    // isOpenSearch:boolean = false;

    constructor(private fb: FormBuilder,private router: Router, private http: HttpClient,private activatedRoute: ActivatedRoute) { 

    }
  
    ngOnInit(): void {
     


    }

    back(): void {
      window.history.back();
    }
  

    // openSearch(){
    //     this.isOpenSearch = true;
    // }

    // closeSearch(){
    //     this.isOpenSearch = false;
    // }
 

    // openSidebar(){
    //   this.isOpenSidebar = true;
  
    // }
  
}
