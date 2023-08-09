import { Component, OnInit,Injector, ViewChild, OnDestroy } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { AdminService } from 'src/root/service/admin/admin.service';
import { EnableDisableClassCourse } from 'src/root/interfaces/admin/enableDisableClassCourse';
import { RegisteredCourses } from 'src/root/interfaces/admin/registeredCourses';
import { Table } from 'primeng/table';
import { OpenAdminSideBar } from '../admin-template/side-bar/adminSide-bar.component';
import { MultilingualComponent, changeLanguage } from 'src/root/root/sharedModule/Multilingual/multilingual.component';
import { Subscription } from 'rxjs';
import { IyizicoService } from 'src/root/service/iyizico.service';
import { MessageService } from 'primeng/api';
import { Constant } from 'src/root/interfaces/constant';
import { AuthService } from 'src/root/service/auth.service';
import { SchoolClassCourseEnum } from 'src/root/Enums/SchoolClassCourseEnum';

@Component({
  selector: 'school-Transactions',
  templateUrl: './schoolTransactions.component.html',
  styleUrls: ['schoolTransactions.component.css'],
  providers: [MessageService]
})
export class SchoolTransactionsComponent extends MultilingualComponent implements OnInit,OnDestroy {

  private _adminService;
  private _iyizicoService;
  private _authService;
  isSubmitted: boolean = false;
  loadingIcon:boolean = false;
  isDataLoaded:boolean = false;
  cloned!:any;
  schoolTransactions: any;
  @ViewChild('dt') table!: Table;
  changeLanguageSubscription!:Subscription;
  gender!:string;
  cancelSchoolSubscriptionId:string = "";
  refundPaymentId:string = "";
  


  constructor(injector: Injector,authService: AuthService,public messageService:MessageService,private fb: FormBuilder,private http: HttpClient,adminService: AdminService,iyizicoService:IyizicoService) {
    super(injector);
    this._adminService = adminService;
    this._iyizicoService = iyizicoService;
    this._authService = authService;
  }

  ngOnInit(): void {
    this._authService.loginState$.next(false);
    this.loadingIcon = true;
    this.gender = localStorage.getItem("gender")??'';
    this.selectedLanguage = localStorage.getItem("selectedLanguage");
    this.translate.use(this.selectedLanguage ?? '');
    
    this._adminService.getAllSchoolTransactions().subscribe((response) => {
      debugger
        this.schoolTransactions = response;
        this.cloned = response.slice(0);
        this.loadingIcon = false;
        this.isDataLoaded = true;
      });  

        if(!this.changeLanguageSubscription){
          this.changeLanguageSubscription = changeLanguage.subscribe(response => {
            this.translate.use(response.language);
          })
        }
      }

      ngOnDestroy(): void {
        if(this.changeLanguageSubscription){
          this.changeLanguageSubscription.unsubscribe();
        }
      }


      search(event: any) {
        this.table.filterGlobal(event.target.value, 'contains');
      }

      openAdminSideBar(){
        OpenAdminSideBar.next({isOpenSideBar:true})
      }

      resetSorting(): void {
        this.table.reset();
        this.schoolTransactions = this.cloned.slice(0);
      }

      getCancelSubscriptionSchoolId(schoolId: string) {
        this.cancelSchoolSubscriptionId = schoolId;
      }

      cancelSubscription(){
        this._iyizicoService.cancelSubscription(this.cancelSchoolSubscriptionId).subscribe((response: any) => {
          debugger
          if(response.errorMessage == Constant.Success){
            var message = "Subscription cenceled successfully"
            this.messageService.add({ severity: 'success', summary: 'Success', life: 3000, detail: message });
          }
          else{
            this.messageService.add({ severity: 'info', summary: 'Info', life: 3000, detail: response.errorMessage });
          }
        });
      }

      getRefundPaymentId(paymentId: string) {
        this.refundPaymentId = paymentId;
      }

      reFundPayment(){
        this._iyizicoService.refundPayment(this.refundPaymentId,SchoolClassCourseEnum.School).subscribe((response: any) => {
        debugger
        if(response.errorMessage == Constant.Success){
          var message = "Refunded successfully"
          this.messageService.add({ severity: 'success', summary: 'Success', life: 3000, detail: message });
        }
        else{
          this.messageService.add({ severity: 'info', summary: 'Info', life: 3000, detail: response.errorMessage });
        }
        });
      }

    }
    
  


   

