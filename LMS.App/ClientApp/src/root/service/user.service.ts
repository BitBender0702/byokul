import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core"; 
import { Router } from "@angular/router";
import { Observable, Subject } from "rxjs";
import { environment } from "src/environments/environment";
import { FollowUnfollow } from "../interfaces/FollowUnfollow";


@Injectable({providedIn: 'root'})

export class UserService{
    token:string = localStorage.getItem("jwt")?? '';
    private headers!: HttpHeaders;
    get apiUrl(): string {
        return environment.apiUrl;
      }
  constructor(private router: Router, private http: HttpClient) {
    this.headers = new HttpHeaders().set("Authorization", "Bearer " + this.token);
    }
    
    getSidebarInfo(token?:string):Observable<any>{
        if(this.token == ""){
            this.token = localStorage.getItem("jwt")?? '';
            this.headers = new HttpHeaders().set("Authorization", "Bearer " + this.token);
        }
        return this.http.get(`${this.apiUrl}/userdashboard/dashboardDetails`,{
            headers: this.headers
          });
    }

    getUserById(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/getUser` + '?userId=' + userId);
    }

    getLanguageList():Observable<any>{
        return this.http.get(`${this.apiUrl}/class/languageList`,{
            headers: this.headers
          });
    }

    saveUserLanguages(addLanguages:any):Observable<any>{
        return this.http.post(`${this.apiUrl}/users/saveUserLanguages`,addLanguages,{headers: this.headers});
    }

    deleteUserLanguage(deletelanguages:any):Observable<any>{
        return this.http.post(`${this.apiUrl}/users/deleteUserLanguage`,deletelanguages,{headers: this.headers});
    }

    saveUserFollower(followUnfollowUser:FollowUnfollow):Observable<any>{
        return this.http.post(`${this.apiUrl}/users/followUnfollowUser`,followUnfollowUser,{headers: this.headers});
    }

    getUserEditDetails(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/getUserEditDetails` + '?userId=' + userId,{headers: this.headers});
    }

    editUser(credentials:any): Observable<any> {
        return this.http.post(`${this.apiUrl}/users/updateUser`, credentials,{headers: this.headers});
    }

    getUserFollowers(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/userFollowers` + '?userId=' + userId);
    }

    banFollower(followerId:any): Observable<any> {
        return this.http.post(`${this.apiUrl}/users/banFollower` + '?followerId=' + followerId,'',{headers: this.headers});
    }

    getUser(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/getBasicUserInfo` + '?userId=' + userId);

    }

    getMyFeed(postType:number,pageNumber:number):Observable<any>{
        //return this.http.get(`${this.apiUrl}/users/myFeed`);
        let queryParams = new HttpParams().append("postType",postType).append("pageNumber",pageNumber);
        return this.http.get(`${this.apiUrl}/users/myFeed`, {params:queryParams, headers: this.headers});

    }

    getGlobalFeed(postType:number, pageNumber:number):Observable<any>{
        //return this.http.get(`${this.apiUrl}/users/globalFeed`);
        let queryParams = new HttpParams().append("postType",postType).append("pageNumber",pageNumber);
        return this.http.get(`${this.apiUrl}/users/globalFeed`, {params:queryParams, headers: this.headers});

    }

    saveUserPreference(preferenceString:string): Observable<any> {
        return this.http.post(`${this.apiUrl}/users/saveUserPreference` + '?preferenceString=' + preferenceString,'');
    }

    shareDataSubject = new Subject<any>(); //Decalring new RxJs Subject
 
     sendDataToOtherComponent(userId:string){
      this.shareDataSubject.next(userId);
     }

     getPostsByUserId(userId:string,pageNumber:number):Observable<any>{
        let queryParams = new HttpParams().append("userId",userId).append("pageNumber",pageNumber);
        return this.http.get(`${this.apiUrl}/users/getPostsByUserId`, {params:queryParams,headers: this.headers});
    }

    getReelsByUserId(userId:string,pageNumber:number):Observable<any>{
        let queryParams = new HttpParams().append("userId",userId).append("pageNumber",pageNumber);
        return this.http.get(`${this.apiUrl}/users/getReelsByUserId`, {params:queryParams,headers: this.headers});
    }

    getNotificationSettings(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/notifications/getNotificationSettings` + '?userId=' + userId, {headers: this.headers});
    }
    
    getCertificatePdf(certificateName:string,from:number){
        let queryParams = new HttpParams().append("certificateName",certificateName).append("from",from);
        return this.http.get(`${this.apiUrl}/users/getCertificatePdf`, {params:queryParams,headers: this.headers}
        );
        

        
    }

    getUserByEmail(email:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/getUserByEmail` + '?email=' + email);
    }

    getCountryList():Observable<any>{
        return this.http.get(`${this.apiUrl}/users/countryList`);
    }

    getCityList(countryId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/cityList` + '?countryId=' + countryId);
    }

    

}
