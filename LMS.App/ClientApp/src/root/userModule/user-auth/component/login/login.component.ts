import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, ElementRef, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import axios from 'axios';
import { AuthService } from 'src/root/service/auth.service';
import { AuthenticatedResponse } from 'src/root/interfaces/auth_response';
import { LoginModel } from 'src/root/interfaces/login';
import { RolesEnum } from 'src/root/RolesEnum/rolesEnum';
import { MultilingualComponent } from 'src/root/root/sharedModule/Multilingual/multilingual.component';
import { BehaviorSubject, finalize, Subject } from 'rxjs';
import { MessageService } from 'primeng/api';
import { SignalrService } from 'src/root/service/signalr.service';
import { UserService } from 'src/root/service/user.service';
import { stringify } from 'querystring';
import { registrationResponse } from '../register/register.component';
import { forgotPassResponse } from '../forget-password/forget-password.component';
import { confirmEmailResponse } from '../confirmEmail/confirmEmail.component';
import { resetPassResponse } from 'src/root/root/sharedModule/reset-password.component';
import { changePassResponse } from '../change-password/change-password.component';
import { setPassResponse } from '../set-password/set-password.component';
import { TranslateService } from '@ngx-translate/core';
import { ResendEmailModel } from 'src/root/interfaces/resendEmailModel';

export const dashboardResponse =new Subject<{token:string}>(); 
export const feedState =new BehaviorSubject <string>('myFeed');  
declare var $: any;


@Component({
    selector: 'login-root',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css'],
    providers: [MessageService]
  })

export class LoginComponent extends MultilingualComponent implements OnInit {
    invalidLogin!: boolean;
    loginForm!:FormGroup;
    EMAIL_PATTERN = '[a-zA-Z0-9]+?(\\.[a-zA-Z0-9]+)*@[a-zA-Z]+\\.[a-zA-Z]{2,3}';
    isSubmitted: boolean = false;
    user: any = {};
    selectedLanguage:any;
    loadingIcon:boolean = false;
    isRegister!:boolean;
    isConfirmEmail!:boolean;
    private _authService;
    isForgotPassSent!: boolean;
    isResetpassword!: boolean;
    isSetPassword!: boolean;
    isChangePassword!: boolean;
    isPasswordVisible:boolean=false;
    @ViewChild('passwordInput') passwordInput!: ElementRef<HTMLInputElement>;
  


    
    constructor(injector: Injector, private translateService: TranslateService, public messageService:MessageService,private fb: FormBuilder,private router: Router,private signalRService: SignalrService, 
      private userService: UserService,
      private http: HttpClient,authService:AuthService,private route: ActivatedRoute,private cd: ChangeDetectorRef) { 
      super(injector);
      this._authService = authService;
    }
  
    ngOnInit(): void {   
      debugger
      if(localStorage.getItem("jwt")){
        this.router.navigate([`/user/userFeed`]);
      }

      
      this._authService.loginState$.next(false);
      this.selectedLanguage = localStorage.getItem("selectedLanguage");
      if(this.selectedLanguage == null || this.selectedLanguage == ""){
        localStorage.setItem("selectedLanguage","en");
      }
      this.loginForm = this.fb.group({
        email: this.fb.control('', [Validators.required,Validators.pattern(this.EMAIL_PATTERN)]),
        password: this.fb.control('', [Validators.required])
      });

      try {
        var result = this._authService.getBigBlueButton();
        console.log(result);
      } catch (error) {
         console.log(error);   
      }

      registrationResponse.subscribe(response => {
        this.isRegister = response;
      });

      forgotPassResponse.subscribe(response => {
        this.isForgotPassSent = response;
      });

      resetPassResponse.subscribe(response => {
        this.cd.detectChanges();
        this.isResetpassword = response;
        if(this.isResetpassword){
          const translatedMessage = this.translateService.instant('PasswordResetSuccessfully');
          const translatedSummary = this.translateService.instant('Success');
          this.messageService.add({severity: 'success',summary: translatedSummary,life: 3000,detail: translatedMessage,
          });        
        }
      });

      changePassResponse.subscribe(response => {
        this.cd.detectChanges();
        this.isChangePassword = response;
        if(this.isChangePassword){
          const translatedMessage = this.translateService.instant('PasswordChangedSuccessfully');
          const translatedSummary = this.translateService.instant('Success');
          this.messageService.add({severity: 'success',summary: translatedSummary,life: 3000,detail: translatedMessage,
          });        
        }
      });

      setPassResponse.subscribe(response => {
        this.cd.detectChanges();
        this.isSetPassword = response;
        if(this.isSetPassword){
          const translatedMessage = this.translateService.instant('PasswordSetSuccessfully');
          const translatedSummary = this.translateService.instant('Success');
          this.messageService.add({severity: 'success',summary: translatedSummary,life: 3000,detail: translatedMessage,
          });       
        }
      });

      confirmEmailResponse.subscribe(response => {
        this.cd.detectChanges();
        if(response != ''){
          this.isConfirmEmail = true;
          this.loginForm.controls.email.setValue(response);     
        }
        else{
          this.isConfirmEmail = false;
        }
        if(this.isConfirmEmail){
          const translatedMessage = this.translateService.instant('EmailConfirmedSuccessfully');
          const translatedSummary = this.translateService.instant('Success');
          this.messageService.add({severity: 'success',summary: translatedSummary,life: 3000,detail: translatedMessage,
          });     
        }
      });
    }
  
    onPasswordShow(){
      this.isPasswordVisible=true;
    }
  
    onPasswordHide(){
      this.isPasswordVisible=false;
    }
    
    login(): void {
      this.isSubmitted = true;
      if (!this.loginForm.valid) {
        return;}
      this.loadingIcon = true;
      this.user = this.loginForm.value;
      this._authService.loginUser(this.user).pipe(finalize(()=> this.loadingIcon= false)).subscribe({
        next: (response: AuthenticatedResponse) => {
          if(response.errorMessage == "This email is not registered"){
            this._authService.loginState$.next(false);
            this.loginForm.setErrors({ unauthenticated: true });
          }

          if(response.errorMessage == "The password you entered is incorrect"){
            this._authService.loginState$.next(false);
            this.loginForm.setErrors({ incorrectPassword: true });
          }

          if(response.errorMessage == "email not confirm"){
            localStorage.setItem("email",this.user.email);
            $("#resend-email").modal('show');
            this._authService.loginState$.next(false);
            this.loginForm.setErrors({ emailNotConfirmed: true });
            
          }
          if(response.errorMessage != "This email is not registered" && response.errorMessage != "email not confirm" && response.errorMessage != "The password you entered is incorrect"){
            this.isSubmitted = false;
            this.loadingIcon = false;
            this._authService.loginState$.next(true);
        const token = response.token;
        localStorage.setItem("jwt", token); 
        const userpermissions = response.userPermissions;
        localStorage.setItem("userPermissions", JSON.stringify(userpermissions));
        dashboardResponse.next({token:token});
        this.signalRService.initializeConnection(token);
        this.signalRService.startConnection();
        setTimeout(() => {
          this.signalRService.askServerListener();
        }, 500);
        var decodeData = this.getUserRoles(token);
        if(decodeData.role?.indexOf(RolesEnum.SchoolAdmin) > -1){
          this.router.navigateByUrl(`administration/adminHome`)

        }
        else{
          if(decodeData.isBan == 'True'){
            this.loginForm.setErrors({ banUserMessage: true });
          }

          else{
            this.router.navigate(["../../userFeed"],{ relativeTo: this.route });
          }
        }
          }
        
        },
      error: (err: HttpErrorResponse) => this.invalidLogin = true
      })      
    }

    getUserRoles(token:string): any{
      let jwtData = token.split('.')[1]
      let decodedJwtJsonData = window.atob(jwtData)
      let decodedJwtData = JSON.parse(decodedJwtJsonData)
      return decodedJwtData;
    }
    connectSignalR() : void {
      let token = localStorage.getItem("jwt"); 
      if(!token)
        return;
      this.signalRService.startConnection();
      setTimeout(() => {
              this.signalRService.askServerListener();
              this.signalRService.askServer(this.getUserRoles(token!).jti);
            }, 500);
    }
    

    sendMail:ResendEmailModel = new Object as ResendEmailModel;
    invalidRegister:boolean=true;
    resendEmailToUser(){
      debugger
      var email = localStorage.getItem("email")
        if(email){
          this.sendMail.email = email;
        }
      this._authService.resendEmail(this.sendMail).pipe(finalize(()=> this.loadingIcon = false)).subscribe({
          next: (response: AuthenticatedResponse) => {
            debugger
          if(response.result != "success"){
          }
          else{
          this.isSubmitted = false;
          const token = response.token;
          localStorage.setItem("jwt", token); 
          this.invalidRegister = false;
          this.router.navigateByUrl("user/auth/login");
          registrationResponse.next(true); 
        }
        },
        error: (err: HttpErrorResponse) => {}
      })
    }
    
  }
