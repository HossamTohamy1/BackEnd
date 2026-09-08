export type ManagedOffer = {
  id: string
  title: string
  description: string
  duration: string
  image: string
  createdAt: string
  updatedAt: string
}

const OFFERS_STORAGE_KEY = 'loxx-managed-offers-v1'
const OFFERS_EVENT = 'loxx:managed-offers-change'

function isManagedOffer(value: unknown): value is ManagedOffer {
  if (!value || typeof value !== 'object') return false
  const offer = value as Partial<ManagedOffer>

  return Boolean(
    typeof offer.id === 'string' &&
    typeof offer.title === 'string' &&
    typeof offer.description === 'string' &&
    typeof offer.duration === 'string' &&
    typeof offer.image === 'string' &&
    typeof offer.createdAt === 'string' &&
    typeof offer.updatedAt === 'string',
  )
}

function emitOffersChange(): void {
  if (typeof window === 'undefined') return
  window.dispatchEvent(new CustomEvent(OFFERS_EVENT))
}

export function getManagedOffers(): ManagedOffer[] {
  if (typeof window === 'undefined') return []

  try {
    const parsed = JSON.parse(window.localStorage.getItem(OFFERS_STORAGE_KEY) || '[]') as unknown
    if (!Array.isArray(parsed)) return []

    const seen = new Set<string>()
    return parsed.filter(isManagedOffer).filter(offer => {
      if (seen.has(offer.id)) return false
      seen.add(offer.id)
      return true
    })
  } catch {
    return []
  }
}

function saveManagedOffers(offers: ManagedOffer[]): void {
  window.localStorage.setItem(OFFERS_STORAGE_KEY, JSON.stringify(offers))
  emitOffersChange()
}

export function upsertManagedOffer(offer: ManagedOffer): void {
  const current = getManagedOffers()
  const exists = current.some(item => item.id === offer.id)
  const next = exists
    ? current.map(item => item.id === offer.id ? offer : item)
    : [offer, ...current]

  saveManagedOffers(next)
}

export function deleteManagedOffer(offerId: string): void {
  const current = getManagedOffers()
  saveManagedOffers(current.filter(offer => offer.id !== offerId))
}

export function subscribeToManagedOffers(listener: () => void): () => void {
  function handleChange(): void {
    listener()
  }

  function handleStorage(event: StorageEvent): void {
    if (event.key === OFFERS_STORAGE_KEY) listener()
  }

  window.addEventListener(OFFERS_EVENT, handleChange)
  window.addEventListener('storage', handleStorage)

  return () => {
    window.removeEventListener(OFFERS_EVENT, handleChange)
    window.removeEventListener('storage', handleStorage)
  }
}
