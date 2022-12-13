import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core"; 
import { Router } from "@angular/router";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";

@Injectable({providedIn: 'root'})

export class PostService{
    private headers!: HttpHeaders;
    get apiUrl(): string {
        return environment.apiUrl;
      }
    constructor(private router: Router, private http: HttpClient) { 
        this.headers = new HttpHeaders({'Content-Type': 'application/json; charset=utf-8'});
    }
    
    getSchool(schoolId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/school/getBasicSchoolInfo` + '?schoolId=' + schoolId);

    }

    getClass(classId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/class/getBasicClassInfo` + '?classId=' + classId);

    }

    getUser(userId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/users/getBasicUserInfo` + '?userId=' + userId);

    }

    getCourse(courseId:string):Observable<any>{
        return this.http.get(`${this.apiUrl}/course/getBasicCourseInfo` + '?courseId=' + courseId);

    }

    createPost(credentials:any): Observable<any> {
        debugger
        for(var pair of credentials.entries()) {
            console.log(pair[0]+ ', '+ pair[1]);
         }
        return this.http.post(`${this.apiUrl}/posts/savePost`, credentials);
    }
}