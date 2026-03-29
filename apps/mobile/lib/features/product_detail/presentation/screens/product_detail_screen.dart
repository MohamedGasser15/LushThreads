import 'package:flutter/material.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/models/cart.dart';
import 'package:LushThreads/shared/models/product.dart';

class ProductDetailScreen extends StatefulWidget {
  const ProductDetailScreen({super.key});

  @override
  State<ProductDetailScreen> createState() => _ProductDetailScreenState();
}

class _ProductDetailScreenState extends State<ProductDetailScreen> {
  String? _selectedSize;
  int _selectedColorIdx = 0;
  int _qty = 1;
  bool _descExpanded = false;

  @override
  Widget build(BuildContext context) {
    final product = ModalRoute.of(context)!.settings.arguments as Product;
    _selectedSize ??= product.sizes.first;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            Expanded(
              child: SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _buildImageSection(product),
                    _buildInfoSection(product),
                  ],
                ),
              ),
            ),
            _buildBottomBar(product),
          ],
        ),
      ),
    );
  }

  // ── Image ─────────────────────────────────────────────────────────────────
  Widget _buildImageSection(Product p) {
    return Stack(
      children: [
        Container(
          height: 320,
          color: p.bgColor,
          child: Center(
            child: Text(p.emoji, style: const TextStyle(fontSize: 100)),
          ),
        ),
        // Back button
        Positioned(
          top: 12, left: 16,
          child: GestureDetector(
            onTap: () => Navigator.pop(context),
            child: Container(
              width: 38, height: 38,
              decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
              child: const Icon(Icons.arrow_back_ios_new_rounded,
                  size: 16, color: AppColors.textMain),
            ),
          ),
        ),
        // Fav button
        Positioned(
          top: 12, right: 16,
          child: GestureDetector(
            onTap: () => setState(() => p.isFavorite = !p.isFavorite),
            child: Container(
              width: 38, height: 38,
              decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
              child: Icon(
                p.isFavorite ? Icons.favorite : Icons.favorite_border,
                size: 18,
                color: p.isFavorite ? Colors.redAccent : AppColors.hint,
              ),
            ),
          ),
        ),
        if (p.hasDiscount)
          Positioned(
            bottom: 12, left: 16,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(color: AppColors.error,
                  borderRadius: BorderRadius.circular(8)),
              child: Text('-${p.discountPercent}% OFF',
                  style: const TextStyle(color: Colors.white,
                      fontSize: 11, fontWeight: FontWeight.w700)),
            ),
          ),
      ],
    );
  }

  // ── Info ──────────────────────────────────────────────────────────────────
  Widget _buildInfoSection(Product p) {
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Brand + name
          Text(p.brand.toUpperCase(),
              style: AppTextStyles.labelMedium.copyWith(color: AppColors.primary)),
          const SizedBox(height: 4),
          Text(p.name, style: AppTextStyles.titleLarge),
          const SizedBox(height: 10),

          // Price row
          Row(children: [
            Text('\$${p.price.toStringAsFixed(2)}', style: AppTextStyles.priceLarge),
            if (p.hasDiscount) ...[
              const SizedBox(width: 10),
              Text('\$${p.originalPrice!.toStringAsFixed(2)}',
                  style: AppTextStyles.bodyMedium.copyWith(
                      decoration: TextDecoration.lineThrough, color: AppColors.hint)),
            ],
          ]),
          const SizedBox(height: 20),

          // Size selector
          Text('Size', style: AppTextStyles.labelLarge),
          const SizedBox(height: 10),
          Wrap(
            spacing: 8, runSpacing: 8,
            children: p.sizes.map((size) {
              final active = size == _selectedSize;
              return GestureDetector(
                onTap: () => setState(() => _selectedSize = size),
                child: AnimatedContainer(
                  duration: const Duration(milliseconds: 150),
                  width: 48, height: 48,
                  decoration: BoxDecoration(
                    color: active ? AppColors.primary : AppColors.surface,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: active ? AppColors.primary : AppColors.divider),
                  ),
                  child: Center(
                    child: Text(size,
                        style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600,
                            color: active ? Colors.white : AppColors.textMuted)),
                  ),
                ),
              );
            }).toList(),
          ),
          const SizedBox(height: 20),

          // Color selector
          Text('Color', style: AppTextStyles.labelLarge),
          const SizedBox(height: 10),
          Row(
            children: List.generate(p.colors.length, (i) {
              final active = i == _selectedColorIdx;
              return GestureDetector(
                onTap: () => setState(() => _selectedColorIdx = i),
                child: Container(
                  margin: const EdgeInsets.only(right: 10),
                  width: 32, height: 32,
                  decoration: BoxDecoration(
                    color: p.colors[i],
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: active ? AppColors.primary : Colors.transparent,
                      width: 2.5,
                    ),
                  ),
                ),
              );
            }),
          ),
          const SizedBox(height: 20),

          // Quantity
          Row(
            children: [
              Text('Quantity', style: AppTextStyles.labelLarge),
              const Spacer(),
              _qtyBtn(Icons.remove, () { if (_qty > 1) setState(() => _qty--); }),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Text('$_qty', style: AppTextStyles.titleMedium),
              ),
              _qtyBtn(Icons.add, () => setState(() => _qty++)),
            ],
          ),
          const SizedBox(height: 20),

          // Description
          GestureDetector(
            onTap: () => setState(() => _descExpanded = !_descExpanded),
            child: Container(
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: AppColors.surface, borderRadius: BorderRadius.circular(12)),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('Description', style: AppTextStyles.labelLarge),
                      Icon(_descExpanded
                          ? Icons.keyboard_arrow_up : Icons.keyboard_arrow_down,
                          color: AppColors.hint),
                    ],
                  ),
                  if (_descExpanded) ...[
                    const SizedBox(height: 10),
                    Text(p.description, style: AppTextStyles.bodyMedium),
                  ],
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _qtyBtn(IconData icon, VoidCallback onTap) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 36, height: 36,
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: AppColors.divider),
        ),
        child: Icon(icon, size: 18, color: AppColors.textMain),
      ),
    );
  }

  // ── Bottom Bar ────────────────────────────────────────────────────────────
  Widget _buildBottomBar(Product p) {
    return Container(
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 20),
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: AppColors.divider)),
      ),
      child: Row(children: [
        // Wishlist
        Container(
          width: 50, height: 50,
          decoration: BoxDecoration(
            color: AppColors.surface, borderRadius: BorderRadius.circular(14),
            border: Border.all(color: AppColors.divider),
          ),
          child: GestureDetector(
            onTap: () => setState(() => p.isFavorite = !p.isFavorite),
            child: Icon(
              p.isFavorite ? Icons.bookmark : Icons.bookmark_border,
              color: p.isFavorite ? AppColors.primary : AppColors.hint,
            ),
          ),
        ),
        const SizedBox(width: 12),
        // Add to cart
        Expanded(
          child: GestureDetector(
            onTap: () {
              for (var i = 0; i < _qty; i++) {
                CartManager.instance.addItem(p, _selectedSize!);
              }
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: const Text('Added to cart!'),
                  backgroundColor: AppColors.primary,
                  duration: const Duration(seconds: 1),
                  behavior: SnackBarBehavior.floating,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                ),
              );
            },
            child: Container(
              height: 50,
              decoration: BoxDecoration(
                color: AppColors.primary, borderRadius: BorderRadius.circular(14)),
              child: Center(
                child: Text('Add to Cart — \$${(p.price * _qty).toStringAsFixed(2)}',
                    style: AppTextStyles.labelLarge.copyWith(color: Colors.white)),
              ),
            ),
          ),
        ),
      ]),
    );
  }
}
