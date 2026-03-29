import 'package:flutter/material.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/models/product.dart';
import 'package:LushThreads/shared/widgets/app_bottom_nav.dart';
import 'package:LushThreads/shared/widgets/product_card.dart';

class ShopScreen extends StatefulWidget {
  const ShopScreen({super.key});

  @override
  State<ShopScreen> createState() => _ShopScreenState();
}

class _ShopScreenState extends State<ShopScreen> {
  String _selectedCategory = 'All';
  String _selectedBrand    = 'All';
  String _sortBy           = 'Popular';
  final _searchCtrl        = TextEditingController();
  final _scrollCtrl        = ScrollController();
  String _query            = '';

  final List<String> _sortOptions = ['Popular', 'Newest', 'Price ↑', 'Price ↓'];

  List<Product> get _filtered {
    var list = List<Product>.from(allProducts);
    if (_selectedCategory != 'All') {
      list = list.where((p) =>
          p.category.toLowerCase().contains(
              _selectedCategory.toLowerCase().replaceAll("'", ''))).toList();
    }
    if (_selectedBrand != 'All') {
      list = list.where((p) => p.brand == _selectedBrand).toList();
    }
    if (_query.isNotEmpty) {
      list = list.where((p) =>
          p.name.toLowerCase().contains(_query.toLowerCase()) ||
          p.brand.toLowerCase().contains(_query.toLowerCase())).toList();
    }
    switch (_sortBy) {
      case 'Price ↑': list.sort((a, b) => a.price.compareTo(b.price));
      case 'Price ↓': list.sort((a, b) => b.price.compareTo(a.price));
      case 'Newest':  list = list.reversed.toList();
    }
    return list;
  }

  @override
  void dispose() { _searchCtrl.dispose(); _scrollCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: HideOnScrollBottomNav(
          currentIndex: 1,
          scrollController: _scrollCtrl,
          child: Column(
            children: [
              _buildHeader(),
              _buildSearchBar(),
              _buildCategoryChips(),
              _buildBrandAndSort(),
              const SizedBox(height: 4),
              Expanded(child: _buildGrid()),
            ],
          ),
        ),
      ),
    );
  }

  // ── Header ────────────────────────────────────────────────────────────────
  Widget _buildHeader() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 14, 20, 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text('Shop', style: AppTextStyles.titleLarge),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: AppColors.primaryLight,
              borderRadius: BorderRadius.circular(20),
            ),
            child: Row(children: [
              const Icon(Icons.local_fire_department_rounded,
                  size: 14, color: AppColors.primary),
              const SizedBox(width: 4),
              Text('${_filtered.length} items',
                  style: AppTextStyles.labelSmall.copyWith(color: AppColors.primary)),
            ]),
          ),
        ],
      ),
    );
  }

  // ── Search ────────────────────────────────────────────────────────────────
  Widget _buildSearchBar() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
      child: TextField(
        controller: _searchCtrl,
        onChanged: (v) => setState(() => _query = v),
        decoration: InputDecoration(
          hintText: 'Search products, brands...',
          hintStyle: const TextStyle(color: AppColors.hint, fontSize: 14),
          prefixIcon: const Icon(Icons.search, color: AppColors.hint, size: 20),
          suffixIcon: _query.isNotEmpty
              ? GestureDetector(
                  onTap: () { _searchCtrl.clear(); setState(() => _query = ''); },
                  child: const Icon(Icons.close, color: AppColors.hint, size: 18))
              : null,
          filled: true, fillColor: AppColors.surface,
          border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
          contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        ),
      ),
    );
  }

  // ── Category Chips ────────────────────────────────────────────────────────
  Widget _buildCategoryChips() {
    return Padding(
      padding: const EdgeInsets.only(left: 20, top: 10, bottom: 4),
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: allCategories.map((cat) {
            final active = cat == _selectedCategory;
            return GestureDetector(
              onTap: () => setState(() => _selectedCategory = cat),
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 200),
                margin: const EdgeInsets.only(right: 8),
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                decoration: BoxDecoration(
                  color: active ? AppColors.primary : AppColors.surface,
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(cat,
                    style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600,
                        color: active ? Colors.white : AppColors.textMuted)),
              ),
            );
          }).toList(),
        ),
      ),
    );
  }

  // ── Brand + Sort Row ──────────────────────────────────────────────────────
  Widget _buildBrandAndSort() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 0),
      child: Row(
        children: [
          // Brand chips
          Expanded(
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: allBrands.map((brand) {
                  final active = brand == _selectedBrand;
                  return GestureDetector(
                    onTap: () => setState(() => _selectedBrand = brand),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      margin: const EdgeInsets.only(right: 8),
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: active ? AppColors.accent : AppColors.surface,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: active ? AppColors.accent : Colors.transparent),
                      ),
                      child: Text(brand,
                          style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600,
                              color: active ? AppColors.textMain : AppColors.textMuted)),
                    ),
                  );
                }).toList(),
              ),
            ),
          ),
          // Sort button
          GestureDetector(
            onTap: _showSortSheet,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              decoration: BoxDecoration(
                color: AppColors.surface, borderRadius: BorderRadius.circular(12)),
              child: Row(children: [
                const Icon(Icons.sort_rounded, size: 16, color: AppColors.textMuted),
                const SizedBox(width: 4),
                Text(_sortBy,
                    style: AppTextStyles.labelSmall.copyWith(color: AppColors.textMuted)),
              ]),
            ),
          ),
        ],
      ),
    );
  }

  void _showSortSheet() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Sort By', style: AppTextStyles.titleMedium),
            const SizedBox(height: 16),
            ..._sortOptions.map((opt) => ListTile(
              title: Text(opt, style: AppTextStyles.bodyMedium
                  .copyWith(color: opt == _sortBy ? AppColors.primary : AppColors.textMain)),
              trailing: opt == _sortBy
                  ? const Icon(Icons.check_circle_rounded, color: AppColors.primary)
                  : null,
              onTap: () {
                setState(() => _sortBy = opt);
                Navigator.pop(context);
              },
            )),
          ],
        ),
      ),
    );
  }

  // ── Grid ──────────────────────────────────────────────────────────────────
  Widget _buildGrid() {
    final items = _filtered;
    if (items.isEmpty) {
      return Center(
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          const Text('🔍', style: TextStyle(fontSize: 48)),
          const SizedBox(height: 12),
          Text('No products found', style: AppTextStyles.titleMedium),
          const SizedBox(height: 6),
          Text('Try different filters', style: AppTextStyles.bodyMedium),
        ]),
      );
    }
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: GridView.builder(
        controller: _scrollCtrl,
        padding: const EdgeInsets.only(top: 12, bottom: 8),
        itemCount: items.length,
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2, crossAxisSpacing: 12,
          mainAxisSpacing: 12, childAspectRatio: 0.73,
        ),
        itemBuilder: (_, i) => ProductCard(
          product: items[i],
          onCartChanged: () => setState(() {}),
        ),
      ),
    );
  }
}
