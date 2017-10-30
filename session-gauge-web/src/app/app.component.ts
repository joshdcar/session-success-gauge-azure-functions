import { environment } from './../environments/environment.prod';
import { Component } from '@angular/core';
import { Http, Request } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { Subscription } from 'rxjs/Subscription';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent {
  title = 'app';

  public webcam;
  options: any = {
    audio: false,
    video: false,
    width: 375,
    height: 240,
    fallbackMode: 'callback',
    fallbackSrc: 'jscam_canvas_only.swf',
    fallbackQuality: 85,
    cameraType: 'front' || 'back'
  };

  public imageSnapshot;
  active: boolean;

  recordingSubscription: Subscription;

  // Chart
  view: any[] = [700, 400];
  data: any[] = [];
  colorScheme = {
    domain: ['#ed0e0e', '#ed720d', '#6d401b', '#f2ee21', '#15af39', '#1772c1', '#8816c1', '#c115a4']
  };

  constructor(public http: Http) {
    this.loadData();
  }

  takeSnapshot() {
    this.webcam.getBase64()
      .then(base => {
        this.imageSnapshot = base;
        this.postPhoto(base).subscribe(response => {
          this.loadData();
          console.log(response);
        }, error => {
          console.log(error);
        });

      })
      .catch(e => console.error(e));
  }


  startRecording() {
    this.active = true;
    this.recordingSubscription = Observable.interval(20000).subscribe(x => {
      this.takeSnapshot();
    });

  }

  stopRecording() {
    this.recordingSubscription.unsubscribe();
    this.active = false;
  }


  postPhoto(formData: any) {

    const config: any = {
      method: 'post',
      url: environment.functionEndPoint,
      body: formData
    };

    const request = new Request(config);

    return this.http.request(request);

  }

  loadData() {

    const config = {
      method: 'get',
      url: environment.functionEndPoint
    };

    const request = new Request(config);

    return this.http.request(request).subscribe(response => {
      this.data = response.json() as any[];
    });

  }

  onCamError(err) { }

  onCamSuccess(event) { }

  onSelect(event) {
    console.log(event);
  }

}
