import 'package:flutter/material.dart';
import '../../core/theme/app_colors.dart';

class Product {
  final String id;
  final String name;
  final String brand;
  final double price;
  final double? originalPrice;
  final String emoji;
  final Color bgColor;
  final String category;
  final String description;
  final List<String> sizes;
  final List<Color> colors;
  bool isFavorite;
  int cartQty;

  Product({
    required this.id,
    required this.name,
    required this.brand,
    required this.price,
    this.originalPrice,
    required this.emoji,
    required this.bgColor,
    required this.category,
    required this.description,
    required this.sizes,
    required this.colors,
    this.isFavorite = false,
    this.cartQty = 0,
  });

  bool get hasDiscount => originalPrice != null && originalPrice! > price;

  int get discountPercent => hasDiscount
      ? (((originalPrice! - price) / originalPrice!) * 100).round()
      : 0;
}

// ─── Sample Data ──────────────────────────────────────────────────────────────
final List<Product> allProducts = [
  Product(
    id: '1', name: 'Abstract Floral Shirt', brand: 'Zara', price: 29.99,
    originalPrice: 49.99, emoji: '👕', bgColor: AppColors.cardBg1,
    category: "Men's Fashion",
    description: 'A stylish abstract floral short sleeve shirt perfect for casual outings. Made with breathable cotton fabric.',
    sizes: ['S', 'M', 'L', 'XL'], colors: [Colors.white, Colors.blue, AppColors.primary],
  ),
  Product(
    id: '2', name: 'Colorful Floral Sandals', brand: 'H&M', price: 49.99,
    emoji: '👡', bgColor: AppColors.cardBg5,
    category: "Women's Fashion",
    description: 'Vibrant floral leather sandals with a comfortable sole. Great for summer days.',
    sizes: ['37', '38', '39', '40', '41'], colors: [Colors.pink, Colors.orange, Colors.purple],
  ),
  Product(
    id: '3', name: 'Beige Casual Sneakers', brand: 'Adidas', price: 59.99,
    originalPrice: 89.99, emoji: '👟', bgColor: AppColors.cardBg2,
    category: "Men's Fashion",
    description: 'Classic beige sport sneakers with cushioned insoles for all-day comfort.',
    sizes: ['40', '41', '42', '43', '44', '45'], colors: [Colors.white, AppColors.cardBg2, Colors.grey],
    isFavorite: true,
  ),
  Product(
    id: '4', name: 'Floral V-Neck T-Shirt', brand: 'H&M', price: 19.99,
    emoji: '👗', bgColor: AppColors.cardBg4,
    category: "Women's Fashion",
    description: 'A lightweight floral graphic V-neck t-shirt, ideal for casual and semi-formal looks.',
    sizes: ['XS', 'S', 'M', 'L', 'XL'], colors: [Colors.white, Colors.pink, Colors.lightBlue],
  ),
  Product(
    id: '5', name: 'Yellow Crossbody Bag', brand: 'Zara', price: 39.99,
    originalPrice: 59.99, emoji: '👜', bgColor: AppColors.cardBg3,
    category: 'Accessories',
    description: 'A bold yellow crossbody bag with gold-tone hardware. Spacious and stylish.',
    sizes: ['One Size'], colors: [Colors.yellow, Colors.white, Colors.black],
  ),
  Product(
    id: '6', name: 'Bohemian Wedge Sandals', brand: 'Zara', price: 49.99,
    emoji: '👠', bgColor: AppColors.cardBg5,
    category: "Women's Fashion",
    description: 'Bohemian-inspired multicolor wedge sandals with woven straps and a comfortable platform.',
    sizes: ['36', '37', '38', '39', '40'], colors: [Colors.brown, Colors.orange, Colors.red],
  ),
  Product(
    id: '7', name: 'Floral Beanie Hat', brand: 'H&M', price: 14.99,
    emoji: '🧢', bgColor: AppColors.cardBg6,
    category: "Kids Fashion",
    description: 'A cute bohemian floral beanie for kids, soft and warm for cooler days.',
    sizes: ['S', 'M'], colors: [Colors.purple, Colors.pink, Colors.teal],
  ),
  Product(
    id: '8', name: 'Blue Floral Shirt', brand: 'Zara', price: 32.99,
    originalPrice: 45.99, emoji: '👔', bgColor: AppColors.cardBg4,
    category: "Men's Fashion",
    description: 'A crisp blue floral short-sleeve shirt, perfect for summer events.',
    sizes: ['S', 'M', 'L', 'XL', 'XXL'], colors: [Colors.blue, Colors.white, Colors.indigo],
  ),
];

final List<String> allCategories = ['All', "Men's", "Women's", 'Kids', 'Accessories', 'Sale'];
final List<String> allBrands     = ['All', 'Zara', 'H&M', 'Adidas', 'Nike'];
