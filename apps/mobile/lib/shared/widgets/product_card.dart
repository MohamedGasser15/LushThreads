import 'package:flutter/material.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_text_styles.dart';
import '../../core/constants/app_routes.dart';
import '../models/product.dart';
import '../models/cart.dart';

class ProductCard extends StatefulWidget {
  final Product product;
  final VoidCallback? onCartChanged;

  const ProductCard({super.key, required this.product, this.onCartChanged});

  @override
  State<ProductCard> createState() => _ProductCardState();
}

class _ProductCardState extends State<ProductCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _cartCtrl;
  late Animation<double> _cartScale;

  @override
  void initState() {
    super.initState();
    _cartCtrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 300));
    _cartScale = TweenSequence([
      TweenSequenceItem(tween: Tween(begin: 1.0, end: 1.35), weight: 40),
      TweenSequenceItem(tween: Tween(begin: 1.35, end: 0.9), weight: 30),
      TweenSequenceItem(tween: Tween(begin: 0.9, end: 1.0), weight: 30),
    ]).animate(CurvedAnimation(parent: _cartCtrl, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _cartCtrl.dispose();
    super.dispose();
  }

  void _toggleFav() => setState(() => widget.product.isFavorite = !widget.product.isFavorite);

  void _addToCart() {
    _cartCtrl.forward(from: 0);
    CartManager.instance.addItem(widget.product, widget.product.sizes.first);
    widget.onCartChanged?.call();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(children: [
          const Icon(Icons.check_circle_outline_rounded,
              color: Colors.white, size: 18),
          const SizedBox(width: 8),
          Text('${widget.product.name} added!'),
        ]),
        backgroundColor: AppColors.primary,
        duration: const Duration(milliseconds: 1500),
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.fromLTRB(16, 0, 16, 12),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final p = widget.product;
    return GestureDetector(
      onTap: () => Navigator.pushNamed(context, AppRoutes.productDetail, arguments: p),
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ── Image ──────────────────────────────────────────────────────
            Expanded(
              child: Stack(
                children: [
                  Container(
                    decoration: BoxDecoration(
                      color: p.bgColor,
                      borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                    ),
                    child: Center(
                      child: Text(p.emoji, style: const TextStyle(fontSize: 48)),
                    ),
                  ),
                  if (p.hasDiscount)
                    Positioned(
                      top: 8, left: 8,
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                        decoration: BoxDecoration(
                          color: AppColors.error,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text('-${p.discountPercent}%',
                            style: const TextStyle(fontSize: 9, fontWeight: FontWeight.w700,
                                color: Colors.white)),
                      ),
                    ),
                  Positioned(
                    top: 8, right: 8,
                    child: GestureDetector(
                      onTap: _toggleFav,
                      child: Container(
                        width: 30, height: 30,
                        decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
                        child: Icon(
                          p.isFavorite ? Icons.favorite : Icons.favorite_border,
                          size: 16,
                          color: p.isFavorite ? Colors.redAccent : AppColors.hint,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            // ── Info ───────────────────────────────────────────────────────
            Padding(
              padding: const EdgeInsets.all(10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(p.brand.toUpperCase(),
                      style: AppTextStyles.labelSmall.copyWith(color: AppColors.primary)),
                  const SizedBox(height: 2),
                  Text(p.name, maxLines: 2, overflow: TextOverflow.ellipsis,
                      style: AppTextStyles.labelMedium),
                  const SizedBox(height: 6),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('\$${p.price.toStringAsFixed(2)}',
                              style: AppTextStyles.priceSmall),
                          if (p.hasDiscount)
                            Text('\$${p.originalPrice!.toStringAsFixed(2)}',
                                style: AppTextStyles.bodySmall.copyWith(
                                  decoration: TextDecoration.lineThrough,
                                  color: AppColors.hint,
                                  fontSize: 10,
                                )),
                        ],
                      ),
                      GestureDetector(
                        onTap: _addToCart,
                        child: ScaleTransition(
                          scale: _cartScale,
                          child: Container(
                            width: 28, height: 28,
                            decoration: const BoxDecoration(
                                color: AppColors.primary, shape: BoxShape.circle),
                            child: const Icon(Icons.add, color: Colors.white, size: 16),
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
