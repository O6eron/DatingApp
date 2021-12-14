import { Component, OnInit } from '@angular/core';
import { map } from 'rxjs/operators';
import { Pagination } from 'src/app/_models/pagination';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  activePhoto: Photo =  {
    id: 0,
    url: "",
    isMain: false,
    isApproved: false,
    knownAs: ""};

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotos();
  }

  getPhotos() {
    this.adminService.getPhotosForApproval()
      .subscribe( photos => this.updatePhotos(photos) );
  }

  approvePhoto(photoId: number) {
    console.log("Approve clicked");
    this.adminService.approvePhoto(photoId).subscribe( photos => this.updatePhotos(photos) );
  }

  rejectPhoto(photoId: number) {
    this.adminService.removePhoto(photoId).subscribe( photos => this.updatePhotos(photos) );
  }

  setActivePhoto(photo: Photo) {
    this.activePhoto = photo;
  }

  private updatePhotos(photos: Photo[]) {
    this.photos = photos;
    this.activePhoto = this.photos[0];
  }
}
