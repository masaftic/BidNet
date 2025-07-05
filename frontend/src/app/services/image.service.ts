import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageService {
  private apiBaseUrl = 'http://localhost:5000';

  /**
   * Transform image URLs to include the API base URL if they're relative paths
   * @param imageUrl The image URL to transform
   * @returns The fully qualified image URL
   */
  getImageUrl(imageUrl: string): string {
    if (!imageUrl) {
      return '';
    }

    if (imageUrl.startsWith('http')) {
      return imageUrl;
    }

    return `${this.apiBaseUrl}${imageUrl}`;
  }
}
