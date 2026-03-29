import 'product.dart';

class CartItem {
  final Product product;
  String selectedSize;
  int quantity;

  CartItem({
    required this.product,
    required this.selectedSize,
    this.quantity = 1,
  });

  double get totalPrice => product.price * quantity;
}

class CartManager {
  CartManager._();
  static final CartManager instance = CartManager._();

  final List<CartItem> _items = [];

  List<CartItem> get items => List.unmodifiable(_items);

  int get totalCount => _items.fold(0, (sum, i) => sum + i.quantity);

  double get subtotal => _items.fold(0.0, (sum, i) => sum + i.totalPrice);

  double get shipping => subtotal > 100 ? 0 : 9.99;

  double get total => subtotal + shipping;

  void addItem(Product product, String size) {
    final existing = _items.where(
      (i) => i.product.id == product.id && i.selectedSize == size,
    );
    if (existing.isNotEmpty) {
      existing.first.quantity++;
    } else {
      _items.add(CartItem(product: product, selectedSize: size));
    }
  }

  void removeItem(int index) => _items.removeAt(index);

  void incrementQty(int index) => _items[index].quantity++;

  void decrementQty(int index) {
    if (_items[index].quantity > 1) {
      _items[index].quantity--;
    } else {
      _items.removeAt(index);
    }
  }

  void clear() => _items.clear();
}
