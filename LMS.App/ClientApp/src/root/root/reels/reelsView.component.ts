import { AfterViewChecked, AfterViewInit, ChangeDetectorRef, Component, ElementRef, HostListener, Injector, Input, OnInit, Renderer2, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { CommentLikeUnlike } from 'src/root/interfaces/chat/commentsLike';
import { CommentViewModel } from 'src/root/interfaces/chat/commentViewModel';
import { NotificationType } from 'src/root/interfaces/notification/notificationViewModel';
import { LikeUnlikePost } from 'src/root/interfaces/post/likeUnlikePost';
import { PostView } from 'src/root/interfaces/post/postView';
import { ChatService } from 'src/root/service/chatService';
import { NotificationService } from 'src/root/service/notification.service';
import { PostService } from 'src/root/service/post.service';
import { ReelsService } from 'src/root/service/reels.service';
import { commentLikeResponse, commentResponse, signalRResponse, SignalrService } from 'src/root/service/signalr.service';
import { UserService } from 'src/root/service/user.service';
import { SharePostComponent } from '../sharePost/sharePost.component';

@Component({
    selector: 'reels-view',
    templateUrl: 'reelsView.component.html',
    styleUrls: ['reelsView.component.css'],
  })
  
  export class ReelsViewComponent implements OnInit, AfterViewInit  {

    private _reelsService;
    private _postService;
    private _chatService;
    private _notificationService;
    //reelId!:string;
    reels:any;
    isOpenSidebar:boolean = false;
    isDataLoaded:boolean = false;
    showCommentsField:boolean = true;
    senderId!:string;
    sender:any;
    messageToGroup!:string;
    userId!:string;
    user:any;
    private _signalRService;
    private _userService;
    currentLikedPostId!:string;
    likesLength!:number;
    isLiked!:boolean;
    likeUnlikePost!: LikeUnlikePost;
    postView!:PostView;
    loginUserId!:string;

    postAttachmentId:any;
    commentViewModel!: CommentViewModel;
    commentLikeCount!:number;
    commentLikeUnlike!:CommentLikeUnlike;

    attachmentId!:any;
    reelPageView:boolean = false;

    commentsScrolled:boolean = false;
    scrollCommentsResponseCount:number = 1;
    commentsLoadingIcon: boolean = false;
    commentsPageNumber:number = 1;


    @ViewChild('groupChatList') groupChatList!: ElementRef;

    constructor(private bsModalService: BsModalService,notificationService:NotificationService,chatService:ChatService,private renderer: Renderer2,public options: ModalOptions,private userService: UserService,postService: PostService,public signalRService: SignalrService,private route: ActivatedRoute,reelsService: ReelsService,private activatedRoute: ActivatedRoute,private cd: ChangeDetectorRef) { 
          this._reelsService = reelsService;
          this._signalRService = signalRService;
          this._userService = userService;
          this._postService = postService;
          this._chatService = chatService;
          this._notificationService = notificationService;
  
      }

    ngOnInit(): void {
        this.getLoginUserId();

        var id = this.route.snapshot.paramMap.get('id');
        if(id != null){
          this.attachmentId = id;
          this.reelPageView = true;
        }

        else{
        this.postAttachmentId = this.options.initialState;
        this.attachmentId = this.postAttachmentId.postAttachmentId;
        }

        this._reelsService.getReelById( this.attachmentId).subscribe((response) => {
            this.reels = response;
            this.isDataLoaded = true;
            this.addPostView(this.reels.post.id);
            this._signalRService.createGroupName(this.reels.id);
          });

          var validToken = localStorage.getItem("jwt");
          if (validToken != null) {
          let jwtData = validToken.split('.')[1]
          let decodedJwtJsonData = window.atob(jwtData)
          let decodedJwtData = JSON.parse(decodedJwtJsonData);
          this.senderId = decodedJwtData.jti;

        this._userService.getUser(this.senderId).subscribe((response) => {
          this.sender = response;
        });

             
      }

          this.InitializeLikeUnlikePost();
          this.InitializePostView();

          commentResponse.subscribe(response => {
            var comment: any[] = this.reels.post.comments;
            var commentObj = {id:response.id,content:response.message,likeCount:0,isCommentLikedByCurrentUser:false,userAvatar:response.senderAvatar};
            comment.push(commentObj);
            this.cd.detectChanges();
            this.groupChatList.nativeElement.scrollTop = this.groupChatList.nativeElement.scrollHeight;
          });

          commentLikeResponse.subscribe(response => {
            var comments: any[] = this.reels.post.comments;
            var reqComment = comments.find(x => x.id == response.commentId);
            if(response.isLike){
              reqComment.likeCount = reqComment.likeCount + 1;
            }
            else{
              reqComment.likeCount = reqComment.likeCount - 1;
            }
          });

          // this.scrollToBottom();
    }

    ngAfterViewInit() {        
      setTimeout(() => this.scrollToBottom());
    } 

  scrollToBottom(): void {
      try {
          this.groupChatList.nativeElement.scrollTop = this.groupChatList.nativeElement.scrollHeight;
      } catch(err) { }                 
  }

    getLoginUserId(){
      var validToken = localStorage.getItem("jwt");
      if (validToken != null) {
        let jwtData = validToken.split('.')[1]
        let decodedJwtJsonData = window.atob(jwtData)
        let decodedJwtData = JSON.parse(decodedJwtJsonData);
        this.loginUserId = decodedJwtData.jti;
      }
    }

    InitializeCommentViewModel(){
      this.commentViewModel = {
        id:'',
        userId: '',
        content:'',
        groupName:'',
        userAvatar:'',
        createdOn:new Date(),
        userName:''
       };

    }

    sendToGroup(){
      var comment: any[] = this.reels.post.comments;
      this.InitializeCommentViewModel();
      this.commentViewModel.userId = this.sender.id;
      this.commentViewModel.groupName = this.reels.id + "_group";
      this.commentViewModel.content = this.messageToGroup;
      this.commentViewModel.userAvatar = this.sender.avatar;
      this.messageToGroup = "";
      this.commentViewModel.id = '00000000-0000-0000-0000-000000000000';
      this._chatService.addComments(this.commentViewModel).subscribe((response) => {
        comment.push(response);
        this.cd.detectChanges();
        this.groupChatList.nativeElement.scrollTop = this.groupChatList.nativeElement.scrollHeight;
        this.commentViewModel.id = response.id;
        this._signalRService.sendToGroup(this.commentViewModel);
        });
      }

    generateChatDiv(response:any,profileImage:string){
      const p: HTMLParagraphElement = this.renderer.createElement('p');

      var li=`<div class="live-message d-flex position-relative align-items-start">
      <img src="${profileImage}" class="comment-user">
      <p class="text_sec1 font_12 fw_400 text-start mb-0">${response.message}</p>
      <button type="button" class="btn bg-transparent text_ltgray font_12 p-0 border-0 reel-like-chat"><img
          src="../../../assets/images/Heart-dark.svg" class="d-block" /> 34</button>
    </div>`
    p.innerHTML =li;
      this.renderer.appendChild(this.groupChatList.nativeElement, p)
    }

    getSenderInfo(userId:string){
      this._userService.getUser(userId).subscribe((response) => {
        this.user = response;
        this.generateChatDiv(response,this.user.avatar);
      });
    }


    showComments(){
        if(this.showCommentsField){
            this.showCommentsField = false;
        }
        else{
            this.showCommentsField = true;
        }
    }

    InitializeLikeUnlikePost(){
      this.likeUnlikePost = {
        postId: '',
        userId: '',
        isLike:false,
        commentId:''
       };

    }

    InitializePostView(){
      this.postView = {
        postId: '',
        userId: ''
       };

    }

    back(): void {
        window.history.back();
      }

    openSidebar(){
        this.isOpenSidebar = true;
    
      }

      likeUnlikePosts(postId:string, isLike:boolean,postType:number,reelId:any,post:any){
        this.currentLikedPostId = postId;
          var likes: any[] = this.reels.post.likes;
          var isLiked = likes.filter(x => x.userId == this.loginUserId && x.postId == postId);
        if(isLiked.length != 0){
          this.isLiked = false;
          this.likesLength = this.reels.post.likes.length - 1;
          this.reels.post.isPostLikedByCurrentUser = false;
        }
        else{
          this.isLiked = true;
          this.likesLength = this.reels.post.likes.length + 1;
          this.reels.post.isPostLikedByCurrentUser = true;

          var notificationType = NotificationType.Likes;
          if(post.title != null){
            var notificationContent = `liked your post(${post.title})`;
          }
          else{
            var notificationContent = "liked your post";
          }
          //this.initializeNotificationViewModel(this.user.id,notificationType,notificationContent);

          this._notificationService.initializeNotificationViewModel(this.reels.user.id,notificationType,notificationContent,this.loginUserId,postId,postType,null,reelId).subscribe((response) => {
           
          });
      
        }

        this.likeUnlikePost.postId = postId;
        this.likeUnlikePost.isLike = isLike;
        this.likeUnlikePost.commentId = '00000000-0000-0000-0000-000000000000'
        this._postService.likeUnlikePost(this.likeUnlikePost).subscribe((response) => {
           this.reels.post.likes = response;
           this.InitializeLikeUnlikePost();
           console.log("succes");
        });
      
      
      }

      likeUnlikeComments(commentId:string, isLike:boolean,isCommentLikedByCurrentUser:boolean,likeCount:number){
        var comment: any[] = this.reels.post.comments;
        var isCommentLiked = comment.find(x => x.id == commentId);
        this.initializeCommentLikeUnlike();
        this.commentLikeUnlike.userId = this.sender.id;
        this.commentLikeUnlike.commentId = commentId;
        this.commentLikeUnlike.groupName = this.reels.id + "_group";
       if(isCommentLiked.isCommentLikedByCurrentUser){
        isCommentLiked.isCommentLikedByCurrentUser = false;
        isCommentLiked.likeCount = isCommentLiked.likeCount - 1;
        this.commentLikeUnlike.isLike = false;
        this.commentLikeUnlike.likeCount = isCommentLiked.likeCount;
       }
       else{
        isCommentLiked.isCommentLikedByCurrentUser = true;
        isCommentLiked.likeCount = isCommentLiked.likeCount + 1;

        this.commentLikeUnlike.isLike = true;
        this.commentLikeUnlike.likeCount = isCommentLiked.likeCount;
       }
       this.signalRService.notifyCommentLike(this.commentLikeUnlike);
      }

      initializeCommentLikeUnlike(){
        this.commentLikeUnlike = {
          commentId:"",
          userId:"",
          likeCount:0,
          isLike:false,
          groupName:""
        }

      }

      addPostView(postId:string){
        
        if(this.loginUserId != undefined){
        this.postView.postId = postId;
        this._postService.postView(this.postView).subscribe((response) => {
          
          console.log('success');
          this.reels.post.views.length = response;
         }); 
        }
      
      }

      close(): void {
        this.bsModalService.hide();
      }

      showCommentsDiv(isShowComments:boolean){
        if(isShowComments){
          this.reels.post.isCommentsDisabled = false;
        }
        else{
          this.reels.post.isCommentsDisabled = true;
        }

        this._postService.enableDisableComments(this.reels.post.id,this.reels.post.isCommentsDisabled).subscribe((response) => {
          
         }); 
      }

      openSharePostModal(postId:string): void {
        const initialState = {
          postId: postId
        };
        this.bsModalService.show(SharePostComponent,{initialState});
      }

    @HostListener('scroll', ['$event'])
    scrollHandler(event: any) {
     const element = event.target;
     if (element.scrollTop === 0) {
        if(!this.commentsScrolled && this.scrollCommentsResponseCount != 0){
            this.commentsScrolled = true;
            this.commentsLoadingIcon = true;
            this.commentsPageNumber++;
            this.getComments();
            }
     }
     
   }

      // getComments(){
      //   this._chatService.getComments(this.attachmentId,this.commentsPageNumber).subscribe((response) => {
      //     this.reels.post.comments = response.concat(this.reels.post.comments);
      //     this.cd.detectChanges();
      //     this.groupChatList.nativeElement.scrollTop = this.groupChatList.nativeElement.clientHeight;
      //     this.commentsLoadingIcon = false;
      //     this.scrollCommentsResponseCount = response.length; 
      //     this.commentsScrolled = false;
      //     });
      // }

      getComments() {
        this._chatService.getComments(this.attachmentId,this.commentsPageNumber).subscribe((response) => {
          this.reels.post.comments = response.concat(this.reels.post.comments);
          this.cd.detectChanges();
          this.commentsLoadingIcon = false;
          this.scrollCommentsResponseCount = response.length; 
          this.commentsScrolled = false;
          // Scroll to the bottom of the chat list with animation
          const chatList = this.groupChatList.nativeElement;
          const chatListHeight = chatList.scrollHeight;
          this.groupChatList.nativeElement.scrollTop = this.groupChatList.nativeElement.clientHeight;
          const scrollOptions = { 
            duration: 300,
            easing: 'ease-in-out'
          };
          chatList.scrollTo({
            top: this.groupChatList.nativeElement.scrollTop,
            left: 0,
            ...scrollOptions
          });
        });
      }


  }
