import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, Resolve, Router} from '@angular/router';
import {AirService} from '../services/air.service';
import {catchError} from 'rxjs/operators';
import {Observable, of} from 'rxjs';

@Injectable()
export class AirResolver implements  Resolve<Air[]> {
  constructor(private airService: AirService, private router: Router) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Air[]>  {
    return this.airService.getData().pipe(
      catchError(error => {
        this.router.navigate(['/']);
        return of(null);
      })
    );
  }
}
