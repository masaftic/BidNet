import { HttpEventType, HttpInterceptorFn } from "@angular/common/http";
import { tap } from "rxjs";


export const LoggingInterceptor: HttpInterceptorFn = (req, next) => {
  console.log('Request URL:', req.url);
  console.log('Request Method:', req.method);
  console.log('Request Headers:', req.headers.keys());
  if (req.body) {
    console.log('Request Body:', req.body);
  } else {
    console.log('No Request Body');
  }

  return next(req).pipe(
    tap({
      next: (event) => {
        if (event.type === HttpEventType.Response) {
          console.log('Response Status:', event.status);
          console.log('Response Body:', event.body);
        }
      },
      error: (error) => {
        console.error('Request failed with error:', error);
      }
    })
  );
}
